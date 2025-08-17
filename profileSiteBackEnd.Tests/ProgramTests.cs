using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using profileSiteBackEnd;
using Xunit;

public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProgramTests(WebApplicationFactory<Program> factory)
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
        var response = await client.PostAsJsonAsync("/api/admin/login", req);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void SetXsrfCookie_SetsCookie()
    {
        var httpContext = new DefaultHttpContext();
        var antiforgeryMock = new Mock<IAntiforgery>();
        antiforgeryMock.Setup(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()))
            .Returns(new AntiforgeryTokenSet("token", "cookie", "form", "header"));

        // Call the static helper via reflection since it's private
        var method = typeof(Program).GetMethod("SetXsrfCookie", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        method.Invoke(null, new object[] { httpContext, antiforgeryMock.Object });

        Assert.True(httpContext.Response.Cookies.ContainsKey("XSRF-TOKEN"));
    }

    [Fact]
    public async Task AntiforgeryFilter_AllowsGet()
    {
        var filter = new AntiforgeryFilter();
        var ctx = new EndpointFilterInvocationContextMock("GET");
        var result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object>("ok"));
        Assert.Equal("ok", result);
    }
}

// Helper mock for EndpointFilterInvocationContext
public class EndpointFilterInvocationContextMock : EndpointFilterInvocationContext
{
    private readonly HttpContext _httpContext;
    public EndpointFilterInvocationContextMock(string method)
    {
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Method = method;
    }
    public override HttpContext HttpContext => _httpContext;
    public override object[] Arguments => Array.Empty<object>();
    public override T GetArgument<T>(int index) => default!;
}