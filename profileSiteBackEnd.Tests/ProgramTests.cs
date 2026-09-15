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
    public async Task AdminLoginGet_ReturnsMethodNotAllowed()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/admin/login");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}