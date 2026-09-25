namespace Railway.Application.DTO;

public sealed class CreateBookingRequest
{
    public Guid CustomerId { get; init; }
    public Guid JourneyId { get; init; }
    public Guid FareId { get; init; }
}