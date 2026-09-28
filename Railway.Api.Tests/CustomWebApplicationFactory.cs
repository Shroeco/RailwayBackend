using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Railway.Api.Tests;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=railway;Username=railway;Password=railway_dev_password";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:RailwayDatabase"] = ConnectionString
                });
        });
    }
}