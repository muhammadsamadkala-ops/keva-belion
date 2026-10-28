using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace KivaBelion.API.Tests;

/// <summary>Settings used by every test server. All values are fake and the database address is unreachable.</summary>
public static class TestSettings
{
    /// <summary>A throw-away signing key that exists only in the test code.</summary>
    public const string JwtKey = "test-only-signing-key-0123456789-abcdefghijklmnopqrstuvwxyz";

    public static void Apply(IWebHostBuilder builder, int loginPermitsPerMinute = 1000)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Port=1;Database=none;Username=none;Password=none;Timeout=1");
        builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost:4200");
        builder.UseSetting("Features:TestMode", "true");
        builder.UseSetting("Features:OrdersEnabled", "false");
        builder.UseSetting("Features:OnlinePaymentsEnabled", "false");
        builder.UseSetting("Jwt:SigningKey", JwtKey);
        builder.UseSetting("RateLimiting:LoginPermitPerMinute", loginPermitsPerMinute.ToString());
    }
}

/// <summary>Starts the API in memory for tests.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => TestSettings.Apply(builder);
}
