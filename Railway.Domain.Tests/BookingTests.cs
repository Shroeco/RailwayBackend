using Railway.Domain.Entities;
using Railway.Domain.Enums;

namespace Railway.Domain.Tests;

public class BookingTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesConfirmedBooking()
    {
        var id = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var journeyId = Guid.NewGuid();
        var fareId = Guid.NewGuid();
        var bookedAt = DateTimeOffset.UtcNow;

        var booking = new Booking(id, customerId, journeyId, fareId, bookedAt);

        Assert.Equal(id, booking.Id);
        Assert.Equal(customerId, booking.CustomerId);
        Assert.Equal(journeyId, booking.JourneyId);
        Assert.Equal(fareId, booking.FareId);
        Assert.Equal(bookedAt, booking.BookedAt);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Booking(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_WithEmptyCustomerId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Booking(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_WithEmptyJourneyId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Booking(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_WithEmptyFareId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Booking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Cancel_OnConfirmedBooking_SetsStatusToCancelled()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        booking.Cancel();

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Cancel_OnAlreadyCancelledBooking_ThrowsException()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        booking.Cancel();

        Assert.Throws<InvalidOperationException>(() =>
            booking.Cancel());
    }
}