using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Threading.RateLimiting;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using profileSiteBackEnd;
using profileSiteBackEnd.Models;
using profileSiteBackEnd.Services;

var builder = WebApplication.CreateBuilder(args);

// Self-hosted on Windows the app runs as a service, so that it survives reboot
// without anyone logging in. This is a no-op when launched from a console or on
// any non-Windows host, so the container build is unaffected.
builder.Host.UseWindowsService(o => o.ServiceName = "ProfileSite");

var isDev = builder.Environment.IsDevelopment();

// Add services to the container.
builder.Services.AddRouting(o => o.LowercaseUrls = true);

// Add EF Core with SQLite or SQL Server
var cs = builder.Configuration.GetConnectionString("Default")
         ?? builder.Configuration.GetConnectionString("DefaultConnection")
         ?? "Data Source=app.db";

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(cs));

// Add Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Image Service
builder.Services.AddScoped<IImageService, ImageService>();

// Add CSRF validation filter
builder.Services.AddScoped<ValidateAntiforgeryHeaderAttribute>();

// Controllers + JSON (camelCase, ignore nulls)
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// (Also apply to Minimal API responses)
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
    o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

//AuthN & AuthZ(cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = isDev ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromMinutes(60);

        o.LoginPath = "/admin/login";
        o.AccessDeniedPath = "/admin/forbidden";


        o.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

//Anti-foregery
builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "X-CSRF-TOKEN";
    o.Cookie.Name = ".AspNetCore.Antiforgery";  // Internal cookie for ASP.NET (not read by JS)
    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = isDev ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Lax;
});

// Data Protection signs the auth cookie, antiforgery tokens, and the passcode
// gate cookie. Keys otherwise live in the container filesystem, which a deploy
// replaces - signing out the admin and invalidating in-flight CSRF tokens on
// every release. Keep them next to the database on the mounted volume.
var keyRingPath = builder.Configuration["Storage:DataProtectionKeys"];
if (!string.IsNullOrWhiteSpace(keyRingPath))
{
    Directory.CreateDirectory(keyRingPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
        .SetApplicationName("profileSiteBackEnd");
}

//Seeder for initial data
builder.Services.AddScoped<DbSeeder>();

//CORS for local dev
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(o =>
{
    o.AddPolicy("vite", p => p
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials());
});

// Trust the hosting platform's reverse proxy for the client IP and scheme.
// Without this every request appears to come from the proxy, which would
// collapse the per-IP rate limiters below into a single shared bucket.
//
// Whoever the app trusts here can forge X-Forwarded-For and so evade those
// rate limiters, which makes the trusted set worth naming explicitly. Set
// ForwardedHeaders__KnownProxies__0 (and __1, ...) when the proxy has a fixed
// address - self-hosting behind a loopback tunnel is the case that matters,
// where 127.0.0.1 and ::1 are the only senders that can reach the app at all.
// Left unset, the set is cleared and any sender is trusted, which is what
// managed platforms (Fly, Render, App Service) require because they front the
// app from addresses we cannot enumerate. That is only safe while the app is
// reachable exclusively through that proxy.
var knownProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>();

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.ForwardLimit = 1;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();

    foreach (var proxy in knownProxies ?? Array.Empty<string>())
    {
        if (IPAddress.TryParse(proxy, out var address))
        {
            o.KnownProxies.Add(address);
        }
        else
        {
            throw new InvalidOperationException(
                $"ForwardedHeaders:KnownProxies contains '{proxy}', which is not an IP address.");
        }
    }
});

// HSTS. The framework default is 30 days with no includeSubDomains, which is
// below what any preload list accepts and leaves a subdomain reachable over
// plaintext. Only tedko.dev and www.tedko.dev exist, both served over HTTPS
// through the Cloudflare tunnel, so includeSubDomains costs nothing here - it
// would need revisiting before pointing an http-only host at a subdomain.
//
// Preload is a one-way door in the sense that removal takes months to
// propagate, but .dev is already preloaded as a whole TLD: browsers refuse
// plaintext for this hostname regardless. The header just makes the intent
// explicit and satisfies hstspreload.org if the domain is ever submitted.
builder.Services.AddHsts(o =>
{
    o.MaxAge = TimeSpan.FromDays(365);
    o.IncludeSubDomains = true;
    o.Preload = true;
});

//Rate limiting for contact form (prevent spam)
builder.Services.AddRateLimiter(o =>
{
    o.AddPolicy("contact", http => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "anon",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 3, // 3 emails per hour per IP
            Window = TimeSpan.FromHours(1)
        }));
        
    o.AddPolicy("login", http => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "anon",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1)
        }));
});


var app = builder.Build();

// Bring the schema up to date on startup. This matters on first deploy, where
// the database lives on a freshly mounted, empty volume. Tests supply their own
// schema and switch this off.
// Seeding stays a separate, opt-in step (Seed:RunOnStartup).
using (var startupScope = app.Services.CreateScope())
{
    if (builder.Configuration.GetValue("Database:MigrateOnStartup", true))
    {
        var db = startupScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    if (builder.Configuration.GetValue<bool>("Seed:RunOnStartup"))
    {
        var seeder = startupScope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync();
    }
}

// Must run before anything that reads the client IP or the request scheme.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    // Without this an unhandled exception returns a bare 500 with no logging.
    app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "application/problem+json";
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError
        });
    }));

    app.UseHsts();
    app.UseHttpsRedirection();
}

// Content-Security-Policy. Every directive below is deliberate; the notes are
// what to check before loosening one.
//
//   default-src 'self'     everything not named explicitly is same-origin only.
//   script-src  'self'     no 'unsafe-inline' and no hash, which holds only
//                          while index.html has no inline <script>. The theme
//                          bootstrap was moved to /theme-init.js for exactly
//                          this reason - putting it back inline breaks the
//                          theme silently, with only a console violation.
//                          static.cloudflareinsights.com is the Cloudflare Web
//                          Analytics beacon, injected at the edge (not in
//                          index.html). Drop it if analytics is turned off.
//   style-src   'unsafe-inline'  framer-motion and React set element styles
//                          through the CSSOM, which CSP does not govern, but
//                          framer-motion also injects <style> elements for
//                          layout animations. Dropping this needs the site
//                          rendered with the policy on and the console clean,
//                          not a reading of the source.
//   img-src     https:     project and uploaded images may be hosted anywhere;
//                          data: covers inlined SVG from lucide-react.
//   connect-src 'self'     the front end only ever calls its own /api; the
//                          cloudflareinsights.com entry is where the analytics
//                          beacon above posts its measurements.
//   frame-ancestors 'none' the modern form of the X-Frame-Options above, which
//                          stays for older browsers that ignore this directive.
//   object-src / base-uri / form-action  close the plugin, <base> rewrite, and
//                          form-post-to-attacker holes that default-src alone
//                          does not cover.
//
// Sent on every response, including API JSON, where it is inert but harmless.
const string contentSecurityPolicy =
    "default-src 'self'; " +
    "script-src 'self' https://static.cloudflareinsights.com; " +
    "style-src 'self' 'unsafe-inline'; " +
    "img-src 'self' data: https:; " +
    "font-src 'self' data:; " +
    "connect-src 'self' https://cloudflareinsights.com; " +
    "object-src 'none'; " +
    "base-uri 'self'; " +
    "form-action 'self'; " +
    "frame-ancestors 'none'; " +
    "upgrade-insecure-requests";

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    ctx.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    ctx.Response.Headers.TryAdd("Content-Security-Policy", contentSecurityPolicy);
    await next();
});

// Static files must come before UseRouting, and UseRouting must be explicit.
//
// WebApplication inserts UseRouting at the very start of the pipeline if it is
// never called, which puts endpoint selection ahead of every middleware here.
// The SPA fallback below is a catch-all, so it gets selected for /assets/app.js
// just as readily as for /projects - and StaticFileMiddleware deliberately does
// nothing once an endpoint is already selected. Every asset then resolves to
// index.html with content type text/html, the module script fails to parse, and
// the site renders as a blank page while every status code is still 200.
app.UseDefaultFiles();

// Be explicit about caching rather than letting the CDN pick a default by file
// extension. Vite fingerprints everything under /assets by content, so those are
// safe to cache forever - a change produces a new filename. index.html must not
// be cached: it is the document that names those filenames, and a stale copy
// points at assets that no longer exist, breaking the site after a deploy while
// every response still returns 200.
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.Context.Request.Path.Value ?? string.Empty;
        var isFingerprinted = path.StartsWith("/assets/", StringComparison.OrdinalIgnoreCase);

        ctx.Context.Response.Headers.CacheControl = isFingerprinted
            ? "public,max-age=31536000,immutable"
            : "no-cache";
    }
});

// Uploaded images live outside wwwroot in production so that a deploy, which
// replaces the application image, does not take the uploads with it.
var uploadsRoot = builder.Configuration["Storage:UploadsRoot"];
if (!string.IsNullOrWhiteSpace(uploadsRoot))
{
    Directory.CreateDirectory(uploadsRoot);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.GetFullPath(uploadsRoot)),
        RequestPath = "/uploads"
    });
}

app.UseRouting();

// CORS, the rate limiter, and authorization all read endpoint metadata, so each
// has to sit after UseRouting. The rate limiter especially: the contact form's
// limit is attached with RequireRateLimiting("contact") on the endpoint, and
// running the middleware before endpoint selection leaves that policy unapplied
// without any error to show for it.
app.UseCors("vite");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ===== Public API =====

// Liveness probe for the hosting platform's health checks.
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/profile", async (AppDbContext db) =>
{
    var p = await db.Profiles.AsNoTracking()
        .Include(x => x.Links)
        .FirstOrDefaultAsync();

    if (p is null) return Results.NotFound();

    var skillRows = await db.Skills.AsNoTracking()
        .Where(s => s.ProfileId == p.Id && s.IsVisible)
        .OrderBy(s => s.Order ?? int.MaxValue)
        .ThenBy(s => s.Name)
        .Select(s => new { s.Name, s.Category })
        .ToListAsync();

    var skills = skillRows.Select(s => s.Name).ToList();

    // Grouped for the Skills section, which renders one labelled block per
    // category. Categories come out in SampleData.SkillCategories order rather
    // than alphabetically or by insertion. Anything with no category, or one not
    // on that list, collects under "Other", so a skill added through the admin
    // panel still appears somewhere instead of vanishing.
    var categoryOrder = SampleData.SkillCategories
        .Select((name, index) => (name, index))
        .ToDictionary(x => x.name, x => x.index, StringComparer.Ordinal);

    var skillGroups = skillRows
        .GroupBy(s => string.IsNullOrWhiteSpace(s.Category) ? "Other" : s.Category!, StringComparer.Ordinal)
        .OrderBy(g => categoryOrder.TryGetValue(g.Key, out var i) ? i : int.MaxValue)
        .ThenBy(g => g.Key, StringComparer.Ordinal)
        .Select(g => new { category = g.Key, items = g.Select(s => s.Name).ToList() })
        .ToList();

    // Project to the shape your React expects (camelCase already configured)
    return Results.Ok(new
    {
        id = p.Id,
        name = p.Name,
        title = p.Title,
        tagline = p.Tagline,
        summary = p.Summary,
        location = p.Location,
        email = p.Email,
        github = p.Github,
        linkedin = p.Linkedin,
        // This projection is hand-written, so a new Profile column is invisible
        // to the front end until it is listed here.
        availabilityNote = p.AvailabilityNote,
        availabilityVisible = p.AvailabilityVisible,
        // Flat list kept alongside the grouped one: it is the older contract and
        // still the simplest thing for any consumer that just wants the names.
        skills,
        skillGroups,
        links = p.Links
    });
});

// Explicit order: this previously returned whatever order the database handed
// back, which put the strongest work wherever insertion happened to leave it.
app.MapGet("/api/projects", async (AppDbContext db) =>
    await db.Projects
    .AsNoTracking()
    .OrderBy(p => p.SortOrder)
    .ThenBy(p => p.Title)
    .ToListAsync());

app.MapGet("/api/experience", async (AppDbContext db) =>
    await db.Experiences.AsNoTracking()
        .OrderByDescending(e => e.Start)
        .ToListAsync());

app.MapGet("/api/education", async (AppDbContext db) =>
    await db.Educations.AsNoTracking()
        .OrderByDescending(e => e.End)
        .ToListAsync());

app.MapGet("/api/certifications", async (AppDbContext db) =>
    await db.Certifications.AsNoTracking()
        .OrderByDescending(c => c.Issued ?? DateTime.MinValue)
        .ToListAsync());

app.MapGet("/api/testimonials", async (AppDbContext db) =>
    await db.Testimonials.AsNoTracking()
        .Where(t => t.IsVisible)
        .OrderBy(t => t.Order ?? int.MaxValue)
        .ThenByDescending(t => t.Date ?? DateTime.MinValue)
        .ToListAsync());

app.MapGet("/api/blog", async (AppDbContext db) =>
    await db.Posts.AsNoTracking()
        .OrderByDescending(p => p.Published)
        .ToListAsync());


//Contact form - Direct email sending
app.MapPost("/api/contact", async (IEmailService emailService, [FromBody] ContactRequest req) =>
{
    // Validation
    if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Message))
        return Results.BadRequest(new { error = "Name, email, and message are required." });

    // Additional validation
    if (req.Name.Length > 100)
        return Results.BadRequest(new { error = "Name is too long (max 100 characters)." });
    
    if (req.Message.Length > 2000)
        return Results.BadRequest(new { error = "Message is too long (max 2000 characters)." });

    // Subject was previously unbounded and flows straight into the mail header.
    if (req.Subject is { Length: > 200 })
        return Results.BadRequest(new { error = "Subject is too long (max 200 characters)." });

    // Basic email format validation
    if (!System.Text.RegularExpressions.Regex.IsMatch(req.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        return Results.BadRequest(new { error = "Please provide a valid email address." });

    // Send email directly
    var success = await emailService.SendContactEmailAsync(req.Name, req.Email, req.Subject, req.Message);
    
    if (success)
    {
        return Results.Ok(new { 
            ok = true, 
            message = "Thank you for your message! I'll get back to you soon." 
        });
    }
    else
    {
        return Results.Problem(
            title: "Email Service Error",
            detail: "Unable to send your message at this time. Please try again later.",
            statusCode: 500
        );
    }
}).RequireRateLimiting("contact");

// Client-side routes (/admin, /projects, ...) get index.html so the React
// router can take over. The constraint keeps /api out of the fallback: an
// unmatched API path is a missing endpoint, not a client-side route.
//
// Excluding /api here rather than mapping a catch-all matters. A catch-all
// matches every method, so it beats a real route whose path matches but whose
// verb does not, turning every 405 in the API into a 404.
// The fallback runs its own static-file middleware, so the no-cache header set
// on the main pipeline does not reach it. Without this, /projects and every
// other client-side route would serve a cacheable index.html even though / does
// not - the same stale-document trap, reachable by a different path.
app.MapFallbackToFile("{*path:regex(^(?!api/).*$)}", "index.html", new StaticFileOptions
{
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache"
});

app.Run();

// ===== DTOs =====
record ContactRequest(string Name, string Email, string? Subject, string Message);

// Exposes Program to WebApplicationFactory<Program> in the test project.
public partial class Program { }
