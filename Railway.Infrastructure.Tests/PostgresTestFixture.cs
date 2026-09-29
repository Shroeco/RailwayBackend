using Microsoft.EntityFrameworkCore;
using Railway.Infrastructure.Data;

namespace Railway.Infrastructure.Tests;

public sealed class PostgresTestFixture
{
    private static readonly string ConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__RailwayDatabase") ?? "Host=localhost;Port=5432;Database=railway;Username=railway;Password=railway_dev_password";

    public async Task EnsureDatabaseAvailableAsync()
    {
        await using var context = CreateContext();

        if (!await context.Database.CanConnectAsync())
        {
            throw new InvalidOperationException("The PostgreSQL integration-test database is unavailable.");
        }
    }

    public RailwayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RailwayDbContext>().UseNpgsql(ConnectionString).Options;

        return new RailwayDbContext(options);
    }
}