namespace Railway.Application.DTO;

public sealed class CarrierJourneyResponse
{
    public string CarrierCode { get; init; } = string.Empty;
    public string ExternalJourneyId { get; init; } = string.Empty;
    
    public Guid OriginStationId { get; init; }
    public Guid DestinationStationId { get; init; }

    public DateTimeOffset DepartureTime { get; init; }
    public DateTimeOffset ArrivalTime { get; init; }

    public decimal Price { get; init; }

    public IReadOnlyList<CarrierJourneyLegResponse> Legs { get; init; } = Array.Empty<CarrierJourneyLegResponse>();
}