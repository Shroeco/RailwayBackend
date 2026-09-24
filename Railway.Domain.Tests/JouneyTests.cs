using Railway.Domain.Entities;

namespace Railway.Domain.Tests;

public class JourneyTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesJourneyWithFullCapacity()
    {
        var id = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var departureTime = DateTimeOffset.UtcNow;
        var arrivalTime = departureTime.AddHours(2);

        var journey = new Journey(id, originStationId, destinationStationId, departureTime, arrivalTime, 100);

        Assert.Equal(id, journey.Id);
        Assert.Equal(originStationId, journey.OriginStationId);
        Assert.Equal(destinationStationId, journey.DestinationStationId);
        Assert.Equal(departureTime, journey.DepartureTime);
        Assert.Equal(arrivalTime, journey.ArrivalTime);
        Assert.Equal(100, journey.Capacity);
        Assert.Equal(100, journey.AvailableSeats);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Journey(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), 100));
    }

    [Fact]
    public void Constructor_WithSameOriginAndDestination_ThrowsException()
    {
        var stationId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() =>
            new Journey(Guid.NewGuid(), stationId, stationId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), 100));
    }

    [Fact]
    public void Constructor_WithArrivalBeforeDeparture_ThrowsException()
    {
        var departureTime = DateTimeOffset.UtcNow;
        var arrivalTime = departureTime.AddHours(-1);

        Assert.Throws<ArgumentException>(() =>
            new Journey(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), departureTime, arrivalTime, 100));
    }

    [Fact]
    public void Constructor_WithZeroCapacity_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Journey(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), 0));
    }

    [Fact]
    public void Constructor_WithNegativeCapacity_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Journey(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), -1));
    }

    [Fact]
    public void ReserveSeat_should_decrease_available_seats()
    {
        var journey = CreateValidJourney();

        journey.ReserveSeat();

        Assert.Equal(4, journey.AvailableSeats);
    }

    [Fact]
    public void ReserveSeat_should_throw_when_no_seats_are_available()
    {
        var journey = CreateValidJourney();

        for (var i = 0; i < 5; i++)
            journey.ReserveSeat();

        Assert.Throws<InvalidOperationException>(() => journey.ReserveSeat());
    }

    private static Journey CreateValidJourney()
    {
        return new Journey(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 5);
    }
}