using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using profileSiteBackEnd.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using profileSiteBackEnd;

var builder = WebApplication.CreateBuilder(args);

//Admin config
var adminUser = builder.Configuration["Admin:Username"] ?? "Admin";
var adminHash = builder.Configuration["Admin:PasswordHash"]; // BCrypt hash
var passcodeHash = builder.Configuration["Private:PasscodeHash"]; //BCrypt hash for private gate
var isDev = builder.Environment.IsDevelopment();

// Add services to the container.
builder.Services.AddRouting(o => o.LowercaseUrls = true);

// Add EF Core with SQLite or SQL Server
var cs = builder.Configuration.GetConnectionString("Default")
         ?? builder.Configuration.GetConnectionString("DefaultConnection")
         ?? "Data Source=app.db";

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(cs));

builder.Services.AddControllers();

//Authentication and Authorization
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

//Antiforgery setup
builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "X-CSRF-TOKEN";
    o.Cookie.Name = "XSRF-TOKEN";
    o.Cookie.HttpOnly = false;
    o.Cookie.SecurePolicy = isDev ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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

//Rate limiting for login endpoint
builder.Services.AddRateLimiter(o =>
{
    o.AddPolicy("login", http => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "anon",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1)
        }));
});

//JSON options
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    o.SerializerOptions.PropertyNameCaseInsensitive = true; // 
    o.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});


//In-memory store(thread-safe)
builder.Services.AddSingleton<Store>();

var app = builder.Build();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Projects.Any()) db.Projects.AddRange(SampleData.GetProjects());
    if (!db.Posts.Any()) db.Posts.AddRange(SampleData.GetPosts());
    if (!db.Profiles.Any()) db.Profiles.Add(SampleData.GetProfile());
    if (!db.Experiences.Any()) db.Experiences.AddRange(SampleData.GetExperience());
    if (!db.Educations.Any()) db.Educations.AddRange(SampleData.GetEducation());
    if (!db.Certifications.Any()) db.Certifications.AddRange(SampleData.GetCertifications());
    db.SaveChanges();
}

app.UseCors("vite");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("vite");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    ctx.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    await next();
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

//===== Helpers =====
static void SetXsrfCookie(HttpContext http, IAntiforgery af)
{
    var tokens = af.GetAndStoreTokens(http);
    http.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
    {
        HttpOnly = false,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        IsEssential = true
    });
}

// ===== Public API =====
app.MapGet("/api/profile", (Store s) => Results.Ok(s.Profile));
app.MapGet("/api/projects", (Store s) => Results.Ok(s.Projects));
app.MapGet("/api/experience", (Store s) => Results.Ok(s.Experience));
app.MapGet("/api/education", (Store s) => Results.Ok(s.Education));
app.MapGet("/api/certifications", (Store s) => Results.Ok(s.Certifications));
app.MapGet("api/blog", (Store s) => Results.Ok(s.Posts));


//Contact endpoint
app.MapPost("/api/contact", ([FromBody] ContactRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Message))
        return Results.BadRequest(new { error = "Name, email, and message are required." });

    // TODO: plug in email (SMTP) or queue here. For now we log.
    Console.WriteLine($"[CONTACT] {DateTime.UtcNow:o} | {req.Name} <{req.Email}> | {req.Message}");
    return Results.Ok(new { ok = true });
}).RequireRateLimiting("login");

// ===== Passcode Gate =====/
app.MapPost("/api/passcode/login", async (HttpContext http, IAntiforgery af) =>
{
    var input = await http.Request.ReadFromJsonAsync<PasscodeDto>();
    if (input is null || string.IsNullOrWhiteSpace(passcodeHash)) return Results.Unauthorized();
    if (!BCrypt.Net.BCrypt.Verify(input.Code ?? string.Empty, passcodeHash)) return Results.Unauthorized();

    http.Response.Cookies.Append("case_access", "1", new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddHours(12)
    });

    SetXsrfCookie(http, af);
    return Results.Ok(new { ok = true });
}).RequireRateLimiting("login");

// Check passcode cookie
app.MapGet("/api/passcode/check", (HttpRequest req) =>
    req.Cookies.ContainsKey("case_access") ? Results.Ok(new { ok = true }) : Results.Unauthorized());

// Logout passcode cookie
app.MapPost("/api/passcode/logout", (HttpResponse res) =>
{
    res.Cookies.Delete("case_access");
    return Results.Ok();
});

// ===== Admin Auth (server-side session) =====

// Accept any CORS preflight aimed at the API
app.MapMethods("/api/{*path}", new[] { "OPTIONS" }, () => Results.Ok())
   .WithDisplayName("CORS Preflight");

// Admin login endpoint
app.MapMethods("/api/admin/login", new[] { "GET", "HEAD" },
    () => Results.StatusCode(StatusCodes.Status405MethodNotAllowed));

// Admin login POST endpoint
app.MapPost("/api/admin/login", async (HttpContext http, IAntiforgery af) =>
{
    if (!http.Request.HasJsonContentType())
        return Results.BadRequest(new { error = "Expected application/json" });

    var dto = await http.Request.ReadFromJsonAsync<LoginDto>(cancellationToken: http.RequestAborted);
    if (dto is null) return Results.BadRequest(new { error = "Invalid payload" });

    bool passwordOk = false;

    if (!string.IsNullOrWhiteSpace(adminHash))
    {
        // If the configured hash is malformed, BCrypt.Verify can throw → catch and treat as invalid
        try { passwordOk = BCrypt.Net.BCrypt.Verify(dto.Password ?? string.Empty, adminHash); }
        catch
        {
            // Optional: log the error so you know the hash is bad, but don't 500 the client
            Console.WriteLine("[ADMIN] Invalid Admin:PasswordHash format; treating as invalid credentials.");
            passwordOk = false;
        }
    }
    else
    {
        // Dev fallback only when no hash configured
        passwordOk = app.Environment.IsDevelopment() && (dto.Password == "test123");
    }

    if (!string.Equals(dto.Username, adminUser, StringComparison.Ordinal) || !passwordOk)
        return Results.Unauthorized();

    var claims = new List<Claim> {
        new(ClaimTypes.Name, adminUser),
        new(ClaimTypes.Role, "Admin")
    };
    var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(id);
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    // Issue CSRF cookie for subsequent POST/PUT/DELETE
    SetXsrfCookie(http, af);
    return Results.Ok(new { user = adminUser });
}).RequireRateLimiting("login");

// Admin logout endpoint
app.MapPost("/api/admin/logout", async (HttpContext http) =>
{
    await http.SignOutAsync();
    return Results.Ok();
});

// Admin: Get current user info
app.MapMethods("/api/admin/me", new[] { "GET", "HEAD" }, async (HttpContext http, IAntiforgery af) =>
{
    if (!http.User.Identity?.IsAuthenticated ?? true) return Results.Unauthorized();
    SetXsrfCookie(http, af);
    return Results.Ok(new
    {
        user = http.User.Identity!.Name,
        roles = http.User.Claims.Where(c => c.Type==ClaimTypes.Role).Select(c => c.Value)
    });
}).RequireAuthorization();

// ===== Admin: Projects Crud(sample) ===== //
var admin = app.MapGroup("/api/admin").RequireAuthorization();

// Admin: Projects CRUD endpoints
admin.MapGet("/projects", (Store s) => Results.Ok(s.Projects));

/// Admin: Get a single project by slug
admin.MapPost("/projects", (Store s, Project p) =>
{
    p.Slug = string.IsNullOrWhiteSpace(p.Slug) ? Guid.NewGuid().ToString("n") : p.Slug;
    s.Projects.Insert(0, p);
    return Results.Ok(p);
}).AddEndpointFilter(new AntiforgeryFilter());

//
admin.MapPut("/project", (Store s, string slug, Project p) =>
{
    var i = s.Projects.FindIndex(x => x.Slug == slug);
    if (i < 0) return Results.NotFound();
    p.Slug = slug;
    s.Projects[i] = p;
    return Results.Ok(p);
}).AddEndpointFilter(new AntiforgeryFilter());

//
admin.MapDelete("/projects/{slug}", (Store s, string slug) =>
{
    s.Projects.RemoveAll(x => x.Slug == slug);
    return Results.NoContent();
}).AddEndpointFilter(new AntiforgeryFilter());

#region <Resume import api endpoint>
// Import endpoint to update in-memory data from JSON (for quick resume-driven updates)
//app.MapPost("/api/import", ([FromBody] ResumeImport import) =>
//{
//    if (import.Profile is not null) profile = import.Profile;
//    if (import.Projects is not null) projects = import.Projects;
//    if (import.Posts is not null) posts = import.Posts;
//    if (import.Experience is not null) experience = import.Experience;
//    if (import.Education is not null) education = import.Education;
//    if (import.Certifications is not null) certifications = import.Certifications;
//    return Results.Ok(new { ok = true });
//});
#endregion

app.Run();
//Minimal anti-forgery filter for non-GETs
class AntiforgeryFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var req = ctx.HttpContext.Request;
        if (HttpMethods.IsGet(req.Method) || HttpMethods.IsHead(req.Method) || HttpMethods.IsOptions(req.Method))
            return next(ctx);
        var af = ctx.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
        await af.ValidateRequestAsync(ctx.HttpContext);
        return await next(ctx);
    }
}

class Store
{
    public Profile Profile { get; } = SampleData.GetProfile();
    public List<Project> Projects { get; } = SampleData.GetProjects();
    public List<Post> Posts { get; } = SampleData.GetPosts();
    public List<Experience> Experience { get; } = SampleData.GetExperience();
    public List<Education> Education { get; } = SampleData.GetEducation();
    public List<Certification> Certifications { get; } = SampleData.GetCertifications();
}

// ===== DTOs & Store =====
record LoginDto(string? Username, string? Password);
record PasscodeDto(string? Code);
record ContactRequest(string Name, string Email, string? Subject, string Message);
public record ResumeImport(
    Profile? Profile,
    List<Project>? Projects,
    List<Post>? Posts,
    List<Experience>? Experience,
    List<Education>? Education,
    List<Certification>? Certifications
);
