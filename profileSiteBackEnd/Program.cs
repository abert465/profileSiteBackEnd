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
    o.Cookie.Name = "XSRF-TOKEN";
    o.Cookie.HttpOnly = false;
    o.Cookie.SecurePolicy = isDev ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Lax;
});

//Seeder for initial data
builder.Services.AddScoped<DbSeeder>();

//Swagger + CORS
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


var app = builder.Build();

app.UseCors("vite");

if (app.Environment.IsDevelopment())
{
    app.UseCors("vite");
    app.UseSwagger();
    app.UseSwaggerUI();
}


//using (var scope = app.Services.CreateScope())
//{
//    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
//    await seeder.SeedAsync(reset: false);
//}



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

app.MapControllers();

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
app.MapGet("/api/profile", async (AppDbContext db) =>
//await db.Profiles.AsNoTracking()
//.Include(p => p.Links)
//.FirstOrDefaultAsync());
{
    var p = await db.Profiles.AsNoTracking()
        .Include(x => x.Links)
        .FirstOrDefaultAsync();

    if (p is null) return Results.NotFound();

    var skills = await db.Skills.AsNoTracking()
        .Where(s => s.ProfileId == p.Id && s.IsVisible)
        .OrderBy(s => s.Order ?? int.MaxValue)
        .ThenBy(s => s.Name)
        .Select(s => s.Name)
        .ToListAsync();

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
        skills,
        links = p.Links
    });
});

app.MapGet("/api/projects", async (AppDbContext db) =>
    await db.Projects
    .AsNoTracking()
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

app.MapGet("/api/blog", async (AppDbContext db) =>
    await db.Posts.AsNoTracking()
        .OrderByDescending(p => p.Published)
        .ToListAsync());


//Contact (public form)
app.MapPost("/api/contact", ([FromBody] ContactRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Message))
        return Results.BadRequest(new { error = "Name, email, and message are required." });

    // TODO: plug in email (SMTP) or queue here. For now we log.
    Console.WriteLine($"[CONTACT] {DateTime.UtcNow:o} | {req.Name} <{req.Email}> | {req.Message}");
    return Results.Ok(new { ok = true });
}).RequireRateLimiting("login");

// ===== Admin Auth (server-side session) =====

// Accept any CORS preflight aimed at the API
app.MapMethods("/api/{*path}", new[] { "OPTIONS" }, () => Results.Ok())
   .WithDisplayName("CORS Preflight");

//Block accidental GET to /login
app.MapMethods("/api/admin/login", new[] { "GET", "HEAD" },
   () => Results.StatusCode(StatusCodes.Status405MethodNotAllowed));

//// Login (POST)
//app.MapPost("/api/admin/login", async (HttpContext http, IAntiforgery af) =>
//{
//    if (!http.Request.HasJsonContentType())
//        return Results.BadRequest(new { error = "Expected application/json" });

//    var dto = await http.Request.ReadFromJsonAsync<LoginDto>(cancellationToken: http.RequestAborted);
//    if (dto is null) return Results.BadRequest(new { error = "Invalid payload" });

//    bool passwordOk = false;

//    if (!string.IsNullOrWhiteSpace(adminHash))
//    {
//        // If the configured hash is malformed, BCrypt.Verify can throw → catch and treat as invalid
//        try { passwordOk = BCrypt.Net.BCrypt.Verify(dto.Password ?? string.Empty, adminHash); }
//        catch
//        {
//            // Optional: log the error so you know the hash is bad, but don't 500 the client
//            Console.WriteLine("[ADMIN] Invalid Admin:PasswordHash format; treating as invalid credentials.");
//            passwordOk = false;
//        }
//    }
//    else
//    {
//        // Dev fallback only when no hash configured
//        passwordOk = app.Environment.IsDevelopment() && (dto.Password == "test123");
//    }

//    if (!string.Equals(dto.Username, adminUser, StringComparison.Ordinal) || !passwordOk)
//        return Results.Unauthorized();

//    var claims = new List<Claim> {
//        new(ClaimTypes.Name, adminUser),
//        new(ClaimTypes.Role, "Admin")
//    };
//    var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//    var principal = new ClaimsPrincipal(id);
//    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

//    // Issue CSRF cookie for subsequent POST/PUT/DELETE
//    SetXsrfCookie(http, af);
//    return Results.Ok(new { user = adminUser });
//}).RequireRateLimiting("login");

//// Admin logout endpoint
//app.MapPost("/api/admin/logout", async (HttpContext http) =>
//{
//    await http.SignOutAsync();
//    return Results.Ok();
//});

//// Admin: Get current user info
//app.MapMethods("/api/admin/me", new[] { "GET", "HEAD" }, async (HttpContext http, IAntiforgery af) =>
//{
//    if (!http.User.Identity?.IsAuthenticated ?? true) return Results.Unauthorized();
//    SetXsrfCookie(http, af);
//    return Results.Ok(new
//    {
//        user = http.User.Identity!.Name,
//        roles = http.User.Claims.Where(c => c.Type==ClaimTypes.Role).Select(c => c.Value)
//    });
//}).RequireAuthorization();

// Admin: Projects CRUD endpoints
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

// ===== DTOs & Store =====
record LoginDto(string? Username, string? Password);
record PasscodeDto(string? Code);
record ContactRequest(string Name, string Email, string? Subject, string Message);

#region <Resume import DTO>
//public record ResumeImport(
//    Profile? Profile,
//    List<Project>? Projects,
//    List<Post>? Posts,
//    List<Experience>? Experience,
//    List<Education>? Education,
//    List<Certification>? Certifications
//);
#endregion
