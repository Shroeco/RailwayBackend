using Microsoft.EntityFrameworkCore;
using Railway.Domain.Entities;
using Railway.Infrastructure.Data;

namespace Railway.Infrastructure.Tests;

public class ConcurrencyTests : IClassFixture<PostgresTestFixture>
{
    // private const string ConnectionString = "Host=localhost;Port=5432;Database=railway;Username=railway;Password=railway_dev_password";

    // private static readonly PostgresTestFixture Fixture = new();

    private readonly PostgresTestFixture _fixture;

    public ConcurrencyTests(PostgresTestFixture fixture)
    {
         _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    [Fact]
    public async Task Concurrent_updates_to_same_journey_should_raise_concurrency_exception()
    {
        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();

        await using (var setupContext = CreateContext())
        {
            var origin = new Station(originStationId, "AAA", "Origin Station");

            var destination = new Station(destinationStationId, "BBB", "Destination Station");

            var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 1);

            setupContext.Stations.AddRange(origin, destination);
            setupContext.Journeys.Add(journey);

            await setupContext.SaveChangesAsync();
        }

        try
        {
            await using var contextA = CreateContext();
            await using var contextB = CreateContext();

            var journeyA = await contextA.Journeys.SingleAsync(journey => journey.Id == journeyId);

            var journeyB = await contextB.Journeys.SingleAsync(journey => journey.Id == journeyId);

            Assert.Equal(1, journeyA.AvailableSeats);
            Assert.Equal(1, journeyB.AvailableSeats);

            journeyA.ReserveSeat();
            journeyB.ReserveSeat();

            await contextA.SaveChangesAsync();

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextB.SaveChangesAsync());
        }
        finally
        {
            await using var cleanupContext = CreateContext();

            var journey = await cleanupContext.Journeys.SingleOrDefaultAsync(journey => journey.Id == journeyId);

            if (journey is not null)
                cleanupContext.Journeys.Remove(journey);

            var stations = await cleanupContext.Stations.Where(station => station.Id == originStationId || station.Id == destinationStationId).ToListAsync();

            cleanupContext.Stations.RemoveRange(stations);

            await cleanupContext.SaveChangesAsync();
        }
    }

    // private static RailwayDbContext CreateContext()
    // {
    //     var options = new DbContextOptionsBuilder<RailwayDbContext>().UseNpgsql(ConnectionString).Options;

    //     return new RailwayDbContext(options);
    // }

    private RailwayDbContext CreateContext()
    {
        return _fixture.CreateContext();
    }
}