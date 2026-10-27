using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace KivaBelion.API.HealthChecks;

/// <summary>Opens a connection to PostgreSQL and runs "SELECT 1". Reveals no details to callers.</summary>
public sealed class DatabaseHealthCheck(IConfiguration configuration, ILogger<DatabaseHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return HealthCheckResult.Unhealthy("Database connection string is not configured.");
        }

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database reachable.");
        }
        catch (Exception ex)
        {
            // Log only the error type, never the connection string.
            logger.LogWarning("Database health check failed: {ErrorType}", ex.GetType().Name);
            return HealthCheckResult.Unhealthy("Database is not reachable.");
        }
    }
}
