namespace Railway.Application.DTO;

public sealed class JourneyResponse
{
    public Guid Id { get; init; }
    public Guid OriginStationId { get; init; }
    public Guid DestinationStationId { get; init; }
    public DateTimeOffset DepartureTime { get; init; }
    public DateTimeOffset ArrivalTime { get; init; }
    public int AvailableSeats { get; init; }

    public IReadOnlyList<JourneyLegResponse> JourneyLegs { get; init; } = Array.Empty<JourneyLegResponse>();

    public IReadOnlyList<FareResponse> Fares { get; init; } = Array.Empty<FareResponse>();
}