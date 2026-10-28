using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KivaBelion.API.Data;

/// <summary>
/// Used ONLY by the "dotnet ef" command-line tool (creating migrations / updating the database).
/// It reads the same settings as the app (appsettings + User Secrets) without starting the web app.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KivaBelionDbContext>
{
    public KivaBelionDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Default is missing. Run: powershell -ExecutionPolicy Bypass -File .\\scripts\\set-dev-secrets.ps1");
        }

        var options = new DbContextOptionsBuilder<KivaBelionDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new KivaBelionDbContext(options);
    }
}
