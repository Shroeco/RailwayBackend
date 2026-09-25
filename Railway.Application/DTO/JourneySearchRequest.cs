namespace Railway.Application.DTO;

public sealed class JourneySearchRequest
{
    public Guid OriginStationId { get; init; }
    public Guid DestinationStationId { get; init; }
    public DateTimeOffset DepartureFrom { get; init; }
    public DateTimeOffset DepartureTo { get; init; }
}