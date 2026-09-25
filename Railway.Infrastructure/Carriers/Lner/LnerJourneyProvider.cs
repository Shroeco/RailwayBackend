using Railway.Application.DTO;
using Railway.Application.Services;

namespace Railway.Infrastructure.Carriers.Lner;

public sealed class LnerJourneyProvider : ICarrierJourneyProvider
{
    public string CarrierCode => "LNER";

    public async Task<IReadOnlyList<CarrierJourneyResponse>> SearchJourneyAsync(Guid originStationId, Guid destinationStationId, DateTimeOffset departureFrom, DateTimeOffset departureTo, CancellationToken cancellationToken = default)
    {
        await Task.Delay(150, cancellationToken);

        var journeys = new List<CarrierJourneyResponse>
        {
            new()
            {
                CarrierCode = CarrierCode,
                ExternalJourneyId = "LNER-2001",
                OriginStationId = originStationId,
                DestinationStationId = destinationStationId,
                DepartureTime = departureFrom.AddMinutes(45),
                ArrivalTime = departureFrom.AddHours(1).AddMinutes(50),
                Price = 52.50m,
                Legs =
                [
                    new CarrierJourneyLegResponse
                    {
                        OriginStationId = originStationId,
                        DestinationStationId = destinationStationId,
                        DepartureTime = departureFrom.AddMinutes(45),
                        ArrivalTime = departureFrom.AddHours(1).AddMinutes(50)
                    }
                ]
            }
        };

        return journeys.Where(journey => journey.DepartureTime >= departureFrom && journey.DepartureTime <= departureTo).ToList();
    }
}