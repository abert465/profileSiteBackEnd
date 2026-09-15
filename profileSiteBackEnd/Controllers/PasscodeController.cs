using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace profileSiteBackEnd.Controllers;

[ApiController]
[Route("api/passcode")]
public class PasscodeController : ControllerBase
{
    #region <private>
    private readonly IConfiguration _cfg;
    private readonly IWebHostEnvironment _env;
    private readonly IAntiforgery _af;
    private readonly IDataProtector _protector;

    private const string AccessCookie = "case_access";

    // The cookie carries a signed expiry rather than a constant. Checking only
    // that the cookie exists would let any caller mint their own access, since
    // a client controls which cookies it sends regardless of HttpOnly.
    private static string BuildToken() =>
        DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds().ToString();

    private bool TokenIsValid(string? protectedToken)
    {
        if (string.IsNullOrEmpty(protectedToken)) return false;
        try
        {
            var expiresAt = long.Parse(_protector.Unprotect(protectedToken));
            return DateTimeOffset.FromUnixTimeSeconds(expiresAt) > DateTimeOffset.UtcNow;
        }
        catch
        {
            // Tampered, truncated, or signed with a retired key.
            return false;
        }
    }

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
    public PasscodeController(IConfiguration cfg, IWebHostEnvironment env, IAntiforgery af,
        IDataProtectionProvider dataProtection)
    {
        _cfg = cfg; _env = env; _af = af;
        _protector = dataProtection.CreateProtector("profileSiteBackEnd.PasscodeGate.v1");
    }
    #endregion

    #region <dtos>
    public record PasscodeDto(string? Code);
    #endregion

    #region <methods>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public IActionResult Login([FromBody] PasscodeDto dto)
    {
        var passcodeHash = _cfg["Private:PasscodeHash"];
        if (string.IsNullOrWhiteSpace(passcodeHash) || dto is null) return Unauthorized();

        bool ok;
        try { ok = BCrypt.Net.BCrypt.Verify(dto.Code ?? string.Empty, passcodeHash); }
        catch { return Unauthorized(); } // malformed hash must not surface as a 500

        if (!ok) return Unauthorized();

        Response.Cookies.Append(AccessCookie, _protector.Protect(BuildToken()), new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(12)
        });

        IssueXsrfCookie();
        return Ok(new { ok = true });
    }

    [HttpGet("check")]
    public IActionResult Check() =>
        TokenIsValid(Request.Cookies[AccessCookie]) ? Ok(new { ok = true }) : Unauthorized();

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AccessCookie);
        return Ok();
    }
    #endregion
}
