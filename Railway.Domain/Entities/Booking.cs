using Railway.Domain.Enums;

namespace Railway.Domain.Entities;

public sealed class Booking
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid JourneyId { get; private set; }
    public Guid FareId { get; private set; }
    public DateTimeOffset BookedAt { get; private set; }
    public BookingStatus Status { get; private set; }

    public Booking(Guid id, Guid customerId, Guid journeyId, Guid fareId, DateTimeOffset bookedAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Booking ID cannot be empty", nameof(id));
        
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));
        
        if (journeyId == Guid.Empty)
            throw new ArgumentException("Journey ID cannot be empty", nameof(journeyId));
        
        if (fareId == Guid.Empty)
            throw new ArgumentException("Fare ID cannot be empty", nameof(fareId));

        Id = id;
        JourneyId = journeyId;
        CustomerId = customerId;
        FareId = fareId;
        BookedAt = bookedAt;
        Status = BookingStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled");

        Status = BookingStatus.Cancelled;
    }
}