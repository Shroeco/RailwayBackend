using Railway.Domain.Entities;

namespace Railway.Domain.Tests;

public class JourneyLegTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesJourneyLeg()
    {
        var id = Guid.NewGuid();
        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var carrierId = Guid.NewGuid();
        var departureTime = DateTimeOffset.UtcNow;
        var arrivalTime = departureTime.AddHours(2);

        var journeyLeg = new JourneyLeg(id, journeyId, originStationId, destinationStationId, departureTime, arrivalTime, carrierId);

        Assert.Equal(id, journeyLeg.Id);
        Assert.Equal(journeyId, journeyLeg.JourneyId);
        Assert.Equal(originStationId, journeyLeg.OriginStationId);
        Assert.Equal(destinationStationId, journeyLeg.DestinationStationId);
        Assert.Equal(departureTime, journeyLeg.DepartureTime);
        Assert.Equal(arrivalTime, journeyLeg.ArrivalTime);
        Assert.Equal(carrierId, journeyLeg.CarrierId);
    }

    [Fact]
    public void Constructor_WithEmptyJourneyId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new JourneyLeg(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_WithSameOriginAndDestination_ThrowsException()
    {
        var stationId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() =>
            new JourneyLeg(Guid.NewGuid(), Guid.NewGuid(), stationId, stationId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_WithArrivalBeforeDeparture_ThrowsException()
    {
        var departureTime = DateTimeOffset.UtcNow;
        var arrivalTime = departureTime.AddHours(-1);

        Assert.Throws<ArgumentException>(() =>
            new JourneyLeg(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), departureTime, arrivalTime, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_WithEmptyCarrierId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new JourneyLeg(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), Guid.Empty));
    }
}