using Microsoft.AspNetCore.Antiforgery;
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
    public PasscodeController(IConfiguration cfg, IWebHostEnvironment env, IAntiforgery af)
    {
        _cfg = cfg; _env = env; _af = af;
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

        var ok = BCrypt.Net.BCrypt.Verify(dto.Code ?? string.Empty, passcodeHash);
        if (!ok) return Unauthorized();

        Response.Cookies.Append("case_access", "1", new CookieOptions
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
        Request.Cookies.ContainsKey("case_access") ? Ok(new { ok = true }) : Unauthorized();

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("case_access");
        return Ok();
    }
    #endregion
}
