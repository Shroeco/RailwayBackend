using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Railway.Infrastructure.Data;

namespace Railway.Api.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly RailwayDbContext _dbContext;

    public DatabaseHealthCheck(RailwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect ? HealthCheckResult.Healthy("PostgreSQL is reachable.") : HealthCheckResult.Unhealthy("PostgreSQL is unreachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL health check failed.", exception);
        }
    }
}