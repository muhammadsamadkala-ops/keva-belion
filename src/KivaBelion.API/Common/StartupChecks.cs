namespace KivaBelion.API.Common;

/// <summary>Checks settings once at startup so mistakes show up immediately with a clear message.</summary>
public static class StartupChecks
{
    public static string[] GetAllowedOrigins(IConfiguration config)
    {
        var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        foreach (var origin in origins)
        {
            var ok = origin != "*"
                     && !origin.EndsWith('/')
                     && Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                     && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                     && uri.AbsolutePath == "/";
            if (!ok)
            {
                throw new InvalidOperationException(
                    $"Cors:AllowedOrigins contains '{origin}'. Use only an origin such as 'https://example.com' " +
                    "(no '*', no path, no trailing slash).");
            }
        }

        return origins;
    }

    public static void Validate(IConfiguration config, IHostEnvironment env, ILogger logger)
    {
        // 1) The database connection string must exist (its value is never logged).
        if (string.IsNullOrWhiteSpace(config.GetConnectionString("Default")))
        {
            throw new InvalidOperationException(
                "The setting ConnectionStrings:Default is missing. On your PC run: " +
                "powershell -ExecutionPolicy Bypass -File .\\scripts\\set-dev-secrets.ps1");
        }

        // 2) CORS origins must be valid (throws a clear message if not).
        var origins = GetAllowedOrigins(config);

        // 3) Admin sign-in settings. The signing key is a secret and is never logged.
        var jwtKey = config["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
        {
            throw new InvalidOperationException(
                "The setting Jwt:SigningKey is missing or shorter than 32 characters. On your PC run: " +
                "powershell -ExecutionPolicy Bypass -File .\\scripts\\set-jwt-secret.ps1");
        }
        if (string.IsNullOrWhiteSpace(config["Jwt:Issuer"]) || string.IsNullOrWhiteSpace(config["Jwt:Audience"]))
        {
            throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience must be set in appsettings.json.");
        }

        // 4) Safety switches. Phase A keeps everything in test mode.
        var testMode = config.GetValue("Features:TestMode", true);
        var ordersEnabled = config.GetValue("Features:OrdersEnabled", false);
        var onlinePayments = config.GetValue("Features:OnlinePaymentsEnabled", false);

        if (onlinePayments)
        {
            throw new InvalidOperationException(
                "Features:OnlinePaymentsEnabled is true, but online payments are not built yet. Set it to false.");
        }

        if (!env.IsDevelopment() && (!testMode || ordersEnabled))
        {
            throw new InvalidOperationException(
                "Outside development, live orders are not allowed yet. Keep Features:TestMode=true and Features:OrdersEnabled=false.");
        }

        logger.LogInformation(
            "Startup checks passed. Environment={Environment}, AllowedOrigins={OriginCount}, TestMode={TestMode}, OrdersEnabled={OrdersEnabled}",
            env.EnvironmentName, origins.Length, testMode, ordersEnabled);
    }
}
