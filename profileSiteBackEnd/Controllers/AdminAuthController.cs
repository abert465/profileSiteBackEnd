using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using profileSiteBackEnd;
using profileSiteBackEnd.Services;
using System.Security.Claims;

namespace profileSiteBackEnd.Controllers;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    #region <private>
    private readonly IConfiguration _cfg;
    private readonly IWebHostEnvironment _env;
    private readonly IAntiforgery _af;
    private readonly ILogger<AdminAuthController> _logger;


    private void IssueXsrfCookie()
    {
        var tokens = _af.GetAndStoreTokens(HttpContext);
        Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            IsEssential = true
        });
    }
    #endregion

    #region <ctor>
    public AdminAuthController(IConfiguration cfg, IWebHostEnvironment env, IAntiforgery af,
        ILogger<AdminAuthController> logger)
    {
        _cfg = cfg; _env = env; _af = af; _logger = logger;
    }
    #endregion

    #region <dtos>
    public record LoginDto(string? Username, string? Password);
    #endregion

    #region <methods>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto is null) return BadRequest(new { error = "Invalid payload" });

        var adminUser = _cfg["Admin:Username"] ?? "Admin";
        var adminHash = _cfg["Admin:PasswordHash"]; // BCrypt hash

        // Fail closed when no hash is configured. There is deliberately no
        // default or development password: set Admin:PasswordHash through
        // user-secrets locally, or Admin__PasswordHash in the environment.
        if (string.IsNullOrWhiteSpace(adminHash))
        {
            _logger.LogError("Admin:PasswordHash is not configured; rejecting all admin logins.");
            return Unauthorized();
        }

        bool passwordOk = false;
        try { passwordOk = BCrypt.Net.BCrypt.Verify(dto.Password ?? string.Empty, adminHash); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin:PasswordHash is not a valid BCrypt hash.");
        }

        if (!string.Equals(dto.Username, adminUser, StringComparison.Ordinal) || !passwordOk)
            return Unauthorized();

        var claims = new List<Claim> {
            new(ClaimTypes.Name, adminUser),
            new(ClaimTypes.Role, "Admin")
        };
        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

        IssueXsrfCookie();
        return Ok(new { user = adminUser });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        IssueXsrfCookie(); // refresh CSRF token for SPA writes
        return Ok(new
        {
            user = User.Identity?.Name,
            roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
        });
    }

    [HttpPost("logout")]
    [Authorize]
    [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Ok();
    }
    #endregion
}