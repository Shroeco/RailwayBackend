namespace Railway.Application.DTO;

public sealed class CarrierJourneyLegResponse
{
    public Guid OriginStationId { get; init; }
    public Guid DestinationStationId { get; init; }

    public DateTimeOffset DepartureTime { get; init; }
    public DateTimeOffset ArrivalTime { get; init; }
}