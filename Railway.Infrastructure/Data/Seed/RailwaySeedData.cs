using Microsoft.EntityFrameworkCore;
using Railway.Domain.Entities;

namespace Railway.Infrastructure.Data.Seed;

public static class RailwaySeedData
{
    public static async Task SeedAsync(RailwayDbContext context)
    {
        if (await context.Stations.AnyAsync())
            return;
        
        var euston = new Station(Guid.NewGuid(), "EUS", "London Euston");

        var birmingham = new Station(Guid.NewGuid(), "BHM", "Birmingham New Street");

        var manchester = new Station(Guid.NewGuid(), "MAN", "Manchester Picadilly");

        var miltonKeynes = new Station(Guid.NewGuid(), "MKC", "Milton Keynes Central");

        var liverpool = new Station(Guid.NewGuid(), "LIV", "Liverpool Lime Street");

        var westCoastRail = new Carrier(Guid.NewGuid(), "WCR", "West Coast Rail");

        var midlandsRail = new Carrier(Guid.NewGuid(), "MRL", "Midlands Rail");

        var eustonToBirminham = new Journey(Guid.NewGuid(), euston.Id, birmingham.Id, DateTimeOffset.UtcNow.AddDays(1).AddHours(8), DateTimeOffset.UtcNow.AddDays(1).AddHours(9), 100);

        var eustonToMiltonKeynes = new Journey(Guid.NewGuid(), euston.Id, miltonKeynes.Id, DateTimeOffset.UtcNow.AddDays(1).AddHours(9), DateTimeOffset.UtcNow.AddDays(1).AddHours(9).AddMinutes(45), 80);

        var eustonToManchester = new Journey(Guid.NewGuid(), euston.Id, manchester.Id, DateTimeOffset.UtcNow.AddDays(1).AddHours(10), DateTimeOffset.UtcNow.AddDays(1).AddHours(12), 100);

        var birminghamToManchester = new Journey(Guid.NewGuid(), birmingham.Id, manchester.Id, DateTimeOffset.UtcNow.AddDays(1).AddHours(11), DateTimeOffset.UtcNow.AddDays(1).AddHours(12).AddMinutes(30), 90);

        var birminghamToLiverpool = new Journey(Guid.NewGuid(), birmingham.Id, liverpool.Id, DateTimeOffset.UtcNow.AddDays(1).AddHours(13), DateTimeOffset.UtcNow.AddDays(1).AddHours(14).AddMinutes(30), 90);

        var eustonToBirminhamLeg = new JourneyLeg(Guid.NewGuid(), eustonToBirminham.Id, euston.Id, birmingham.Id, eustonToBirminham.DepartureTime, eustonToBirminham.ArrivalTime, westCoastRail.Id);

        var eustonToMiltonKeynesLeg = new JourneyLeg(Guid.NewGuid(), eustonToMiltonKeynes.Id, euston.Id, miltonKeynes.Id, eustonToMiltonKeynes.DepartureTime, eustonToMiltonKeynes.ArrivalTime, westCoastRail.Id);

        var eustonToManchesterLeg1 = new JourneyLeg(Guid.NewGuid(), eustonToManchester.Id, euston.Id, birmingham.Id, eustonToManchester.DepartureTime, eustonToManchester.DepartureTime.AddHours(1), westCoastRail.Id);

        var eustonToManchesterLeg2 = new JourneyLeg(Guid.NewGuid(), eustonToManchester.Id, birmingham.Id, manchester.Id, eustonToManchester.DepartureTime.AddHours(1).AddMinutes(5), eustonToManchester.ArrivalTime, midlandsRail.Id);

        var birminghamToManchesterLeg = new JourneyLeg(Guid.NewGuid(), birminghamToManchester.Id, birmingham.Id, manchester.Id, birminghamToManchester.DepartureTime, birminghamToManchester.ArrivalTime, midlandsRail.Id);

        var birminghamToLiverpoolLeg = new JourneyLeg(Guid.NewGuid(), birminghamToLiverpool.Id, birmingham.Id, liverpool.Id, birminghamToLiverpool.DepartureTime, birminghamToLiverpool.ArrivalTime, midlandsRail.Id);

        var fares = new[]
        {
            new Fare(Guid.NewGuid(), eustonToBirminham.Id, "Standard", 35.00m),
            new Fare(Guid.NewGuid(), eustonToBirminham.Id, "Flexible", 55.00m),
            new Fare(Guid.NewGuid(), eustonToMiltonKeynes.Id, "Standard", 18.00m),
            new Fare(Guid.NewGuid(), eustonToMiltonKeynes.Id, "Flexible", 28.00m),
            new Fare(Guid.NewGuid(), eustonToManchester.Id, "Standard", 65.00m),
            new Fare(Guid.NewGuid(), eustonToManchester.Id, "Flexible", 95.00m),
            new Fare(Guid.NewGuid(), birminghamToManchester.Id, "Standard", 30.00m),
            new Fare(Guid.NewGuid(), birminghamToManchester.Id, "Flexible", 45.00m),
            new Fare(Guid.NewGuid(), birminghamToLiverpool.Id, "Standard", 25.00m),
            new Fare(Guid.NewGuid(), birminghamToLiverpool.Id, "Flexible", 40.00m),
        };

        context.Stations.AddRange(euston, birmingham, manchester, miltonKeynes, liverpool);

        context.Carriers.AddRange(westCoastRail, midlandsRail);

        context.Journeys.AddRange(eustonToBirminham, eustonToMiltonKeynes, eustonToManchester, birminghamToManchester, birminghamToLiverpool);

        context.JourneyLegs.AddRange(eustonToBirminhamLeg, eustonToMiltonKeynesLeg, eustonToManchesterLeg1, eustonToManchesterLeg2, birminghamToManchesterLeg, birminghamToLiverpoolLeg);

        context.Fares.AddRange(fares);

        await context.SaveChangesAsync();
    }
}