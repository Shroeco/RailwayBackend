using Microsoft.EntityFrameworkCore;
using Railway.Infrastructure.Data;

namespace Railway.Infrastructure.Tests;

public static class TestDatabaseCleanup
{
    public static async Task RemoveJourneyGraphAsync(RailwayDbContext context, Guid journeyId, Guid customerId, params Guid[] stationIds)
    {
        var bookings = await context.Bookings.Where(b => b.JourneyId == journeyId).ToListAsync();

        context.Bookings.RemoveRange(bookings);

        var fares = await context.Fares.Where(f => f.JourneyId == journeyId).ToListAsync();

        context.Fares.RemoveRange(fares);

        var journey = await context.Journeys.FirstOrDefaultAsync(j => j.Id == journeyId);

        if (journey is not null)
            context.Journeys.Remove(journey);

        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer is not null)
            context.Customers.Remove(customer);

        foreach (var stationId in stationIds)
        {
            var station = await context.Stations.FirstOrDefaultAsync(s => s.Id == stationId);

            if (station is not null)
                context.Stations.Remove(station);
        }

        await context.SaveChangesAsync();
    }
}