using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace KivaBelion.API.Tests;

public class FoundationTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;
    public FoundationTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Live_returns_200()
    {
        var r = await _client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Ready_returns_503_when_database_unreachable()
    {
        var r = await _client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, r.StatusCode);
        var body = await r.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Ping_reports_test_mode_and_orders_disabled()
    {
        var json = await _client.GetFromJsonAsync<JsonElement>("/api/v1/system/ping");
        Assert.Equal("KivaBelion.API", json.GetProperty("service").GetString());
        Assert.True(json.GetProperty("testMode").GetBoolean());
        Assert.False(json.GetProperty("ordersEnabled").GetBoolean());
        Assert.False(json.GetProperty("onlinePaymentsEnabled").GetBoolean());
    }

    [Fact]
    public async Task Unknown_route_returns_404_problem_json()
    {
        var r = await _client.GetAsync("/api/v1/does-not-exist");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Equal("application/problem+json", r.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Unhandled_exception_returns_safe_500()
    {
        var r = await _client.GetAsync("/api/v1/dev/throw");
        Assert.Equal(HttpStatusCode.InternalServerError, r.StatusCode);
        var body = await r.Content.ReadAsStringAsync();
        Assert.Contains("traceId", body);
        Assert.DoesNotContain("   at ", body); // no stack trace
    }

    [Fact]
    public async Task Invalid_request_returns_400_with_field_errors()
    {
        var r = await _client.PostAsJsonAsync("/api/v1/dev/validate", new { name = "A", quantity = 99 });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        var body = await r.Content.ReadAsStringAsync();
        Assert.Contains("quantity", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Valid_request_returns_200()
    {
        var r = await _client.PostAsJsonAsync("/api/v1/dev/validate", new { name = "Test", quantity = 2 });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Cors_allows_configured_origin_only()
    {
        var ok = new HttpRequestMessage(HttpMethod.Get, "/api/v1/system/ping");
        ok.Headers.Add("Origin", "http://localhost:4200");
        var okResp = await _client.SendAsync(ok);
        Assert.True(okResp.Headers.Contains("Access-Control-Allow-Origin"));

        var bad = new HttpRequestMessage(HttpMethod.Get, "/api/v1/system/ping");
        bad.Headers.Add("Origin", "https://evil.example");
        var badResp = await _client.SendAsync(bad);
        Assert.False(badResp.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Correlation_id_is_echoed_when_valid_and_replaced_when_invalid()
    {
        var good = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        good.Headers.Add("X-Correlation-Id", "abc-123");
        var goodResp = await _client.SendAsync(good);
        Assert.Equal("abc-123", goodResp.Headers.GetValues("X-Correlation-Id").Single());

        var bad = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        bad.Headers.Add("X-Correlation-Id", "bad id with spaces!");
        var badResp = await _client.SendAsync(bad);
        Assert.NotEqual("bad id with spaces!", badResp.Headers.GetValues("X-Correlation-Id").Single());
    }

    [Fact]
    public async Task Security_headers_are_present()
    {
        var r = await _client.GetAsync("/health/live");
        Assert.Equal("nosniff", r.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", r.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", r.Headers.GetValues("Referrer-Policy").Single());
    }
}
