using System.Net;
using System.Net.Http.Json;
using profileSiteBackEnd.Tests;
using Xunit;

public class ProgramTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProgramTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProfile_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Contact_MissingFields_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var req = new { Name = "", Email = "", Message = "" };
        var response = await client.PostAsJsonAsync("/api/contact", req);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PasscodeLogin_Invalid_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var req = new { Code = "wrong" };
        var response = await client.PostAsJsonAsync("/api/passcode/login", req);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminLogin_Invalid_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var req = new { Username = "admin", Password = "wrong" };
        var response = await client.PostAsJsonAsync("/api/admin/auth/login", req);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminLoginGet_IsRejected()
    {
        var client = _factory.CreateClient();

        // Login is POST-only. This previously targeted /api/admin/login, which
        // was never a real route - it passed only because of a hand-written 405
        // handler for that fictional path.
        var response = await client.GetAsync("/api/admin/auth/login");

        Assert.False(response.IsSuccessStatusCode);
    }

    [Theory]
    [InlineData("/api/nope")]
    [InlineData("/api/admin/auth/login")]
    public async Task ApiPathsNeverFallBackToTheSpaShell(string path)
    {
        var client = _factory.CreateClient();

        // The SPA fallback serves index.html for client-side routes. If its
        // route constraint stops excluding /api, unknown API paths would answer
        // 200 with an HTML page instead of failing, which is far harder to spot
        // than a 404.
        var response = await client.GetAsync(path);

        Assert.False(response.IsSuccessStatusCode);
        Assert.NotEqual("text/html", response.Content.Headers.ContentType?.MediaType);
    }
}