using Microsoft.AspNetCore.Mvc;

namespace KivaBelion.API.Controllers;

/// <summary>Basic information about the running API. Contains no secrets and no customer data.</summary>
[ApiController]
[Route("api/v1/system")]
public sealed class SystemController(IHostEnvironment environment, IConfiguration configuration) : ControllerBase
{
    [HttpGet("ping")]
    public ActionResult<PingResponse> Ping()
    {
        var response = new PingResponse(
            Service: "KivaBelion.API",
            Version: typeof(SystemController).Assembly.GetName().Version?.ToString() ?? "unknown",
            Environment: environment.EnvironmentName,
            ServerTimeUtc: DateTimeOffset.UtcNow,
            TestMode: configuration.GetValue("Features:TestMode", true),
            OrdersEnabled: configuration.GetValue("Features:OrdersEnabled", false),
            OnlinePaymentsEnabled: configuration.GetValue("Features:OnlinePaymentsEnabled", false));

        return Ok(response);
    }
}

public sealed record PingResponse(
    string Service,
    string Version,
    string Environment,
    DateTimeOffset ServerTimeUtc,
    bool TestMode,
    bool OrdersEnabled,
    bool OnlinePaymentsEnabled);
