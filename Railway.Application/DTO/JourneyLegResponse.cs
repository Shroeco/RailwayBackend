namespace Railway.Application.DTO;

public sealed class JourneyLegResponse
{
    public Guid Id { get; init; }
    public Guid OriginStationId { get; init; }
    public Guid DestinationStationId { get; init; }
    public DateTimeOffset DepartureTime { get; init; }
    public DateTimeOffset ArrivalTime { get; init; }
    public Guid CarrierId { get; init; }
}