using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KivaBelion.API.Data.Entities;
using KivaBelion.API.Security;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace KivaBelion.API.Tests;

/// <summary>A test server whose admin accounts live in memory (no database needed).</summary>
public sealed class AuthApiFactory : WebApplicationFactory<Program>
{
    public InMemoryAdminUserStore Store { get; } = new();
    public int LoginPermitsPerMinute { get; init; } = 1000;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        TestSettings.Apply(builder, LoginPermitsPerMinute);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAdminUserStore>();
            services.AddSingleton<IAdminUserStore>(Store);
        });
    }
}

public class AdminAuthApiTests : IClassFixture<AuthApiFactory>
{
    private const string Password = "Correct-Horse-4-Battery";
    private readonly AuthApiFactory _factory;
    private readonly HttpClient _client;

    public AdminAuthApiTests(AuthApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> LoginAsync(string email, string password = Password)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/admin/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("accessToken").GetString()!;
    }

    private Task<HttpResponseMessage> GetWithToken(string url, string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(request);
    }

    [Fact]
    public async Task Login_succeeds_and_the_token_opens_the_me_endpoint()
    {
        _factory.Store.Seed("login-ok@example.com", Password);
        var token = await LoginAsync("login-ok@example.com");

        var me = await GetWithToken("/api/v1/admin/auth/me", token);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        var json = await me.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("login-ok@example.com", json.GetProperty("email").GetString());
        Assert.Equal("Admin", json.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Login_response_never_contains_the_hash_or_password()
    {
        _factory.Store.Seed("no-leak@example.com", Password);
        var response = await _client.PostAsJsonAsync("/api/v1/admin/auth/login", new { email = "no-leak@example.com", password = Password });
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Password, body);
    }

    [Fact]
    public async Task Wrong_password_and_unknown_email_give_the_same_401()
    {
        _factory.Store.Seed("same-answer@example.com", Password);
        var wrong = await _client.PostAsJsonAsync("/api/v1/admin/auth/login", new { email = "same-answer@example.com", password = "wrong-password-1" });
        var unknown = await _client.PostAsJsonAsync("/api/v1/admin/auth/login", new { email = "ghost@example.com", password = "wrong-password-1" });
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal(await wrong.Content.ReadAsStringAsync().ContinueWith(t => Strip(t.Result)),
                     await unknown.Content.ReadAsStringAsync().ContinueWith(t => Strip(t.Result)));
    }

    // Removes the per-request trace id so two error bodies can be compared.
    private static string Strip(string body)
    {
        var doc = JsonDocument.Parse(body).RootElement;
        return doc.GetProperty("title").GetString() + "|" + doc.GetProperty("status").GetInt32();
    }

    [Fact]
    public async Task Invalid_login_body_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/admin/auth/login", new { email = "not-an-email", password = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Protected_endpoints_return_401_without_a_token()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await GetWithToken("/api/v1/admin/auth/me", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await GetWithToken("/api/v1/admin/auth/admin-check", null)).StatusCode);
    }

    [Fact]
    public async Task Garbage_token_returns_401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await GetWithToken("/api/v1/admin/auth/me", "not.a.token")).StatusCode);
    }

    [Fact]
    public async Task Staff_gets_403_and_admin_gets_200_on_the_admin_only_endpoint()
    {
        _factory.Store.Seed("staff-role@example.com", Password, AdminRole.Staff);
        _factory.Store.Seed("admin-role@example.com", Password, AdminRole.Admin);

        var staffToken = await LoginAsync("staff-role@example.com");
        var adminToken = await LoginAsync("admin-role@example.com");

        Assert.Equal(HttpStatusCode.Forbidden, (await GetWithToken("/api/v1/admin/auth/admin-check", staffToken)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await GetWithToken("/api/v1/admin/auth/admin-check", adminToken)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await GetWithToken("/api/v1/admin/auth/me", staffToken)).StatusCode);
    }

    private static string ForgeToken(string key, DateTime notBefore, DateTime expires, string role = "Admin") =>
        new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = "KivaBelion.API",
            Audience = "KivaBelion.Admin",
            NotBefore = notBefore,
            Expires = expires,
            Claims = new Dictionary<string, object> { ["sub"] = "1", ["email"] = "x@example.com", ["name"] = "X", ["role"] = role },
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)
        });

    [Fact]
    public async Task Token_signed_with_another_key_is_rejected()
    {
        var forged = ForgeToken("some-other-key-0123456789-abcdefghijklmnopqrstuvwxyz", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(10));
        Assert.Equal(HttpStatusCode.Unauthorized, (await GetWithToken("/api/v1/admin/auth/admin-check", forged)).StatusCode);
    }

    [Fact]
    public async Task Expired_token_is_rejected()
    {
        var expired = ForgeToken(TestSettings.JwtKey, DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1));
        Assert.Equal(HttpStatusCode.Unauthorized, (await GetWithToken("/api/v1/admin/auth/admin-check", expired)).StatusCode);
    }

    [Fact]
    public async Task Token_with_valid_key_is_accepted_as_a_control_check()
    {
        var good = ForgeToken(TestSettings.JwtKey, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(10));
        Assert.Equal(HttpStatusCode.OK, (await GetWithToken("/api/v1/admin/auth/admin-check", good)).StatusCode);
    }

    [Fact]
    public async Task Disabled_account_cannot_log_in()
    {
        _factory.Store.Seed("disabled@example.com", Password, active: false);
        var response = await _client.PostAsJsonAsync("/api/v1/admin/auth/login", new { email = "disabled@example.com", password = Password });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Account_locks_after_repeated_failures_through_the_api()
    {
        _factory.Store.Seed("lockme@example.com", Password);
        for (var i = 0; i < AdminAuthService.MaxFailedAttempts; i++)
            await _client.PostAsJsonAsync("/api/v1/admin/auth/login", new { email = "lockme@example.com", password = "wrong-password-1" });

        var response = await _client.PostAsJsonAsync("/api/v1/admin/auth/login", new { email = "lockme@example.com", password = Password });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class LoginRateLimitTests
{
    [Fact]
    public async Task Login_is_rate_limited_per_ip()
    {
        using var factory = new AuthApiFactory { LoginPermitsPerMinute = 3 };
        var client = factory.CreateClient();

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 5; i++)
        {
            var r = await client.PostAsJsonAsync("/api/v1/admin/auth/login", new { email = "a@example.com", password = "wrong-password-1" });
            statuses.Add(r.StatusCode);
        }

        Assert.Equal(3, statuses.Count(s => s == HttpStatusCode.Unauthorized));
        Assert.Equal(2, statuses.Count(s => s == HttpStatusCode.TooManyRequests));
    }
}

public class StartupSafetyTests
{
    [Fact]
    public void App_refuses_to_start_without_a_signing_key()
    {
        using var factory = new ApiFactoryWithoutKey();
        Assert.ThrowsAny<Exception>(() => factory.CreateClient());
    }

    private sealed class ApiFactoryWithoutKey : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            TestSettings.Apply(builder);
            builder.UseSetting("Jwt:SigningKey", "too-short");
        }
    }
}
