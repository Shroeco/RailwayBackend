namespace Railway.Application.DTO;

public sealed class BookingResponse
{
    public Guid Id  { get; init; }
    public Guid CustomerId  { get; init; }
    public Guid JourneyId  { get; init; }
    public Guid FareId  { get; init; }
    public DateTimeOffset BookedAt { get; init; }
    public string Status { get; init; } = string.Empty;
}