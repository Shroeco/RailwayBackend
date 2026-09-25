using Railway.Application.DTO;
using Railway.Application.Exceptions;
using Railway.Application.Services;

namespace Railway.Infrastructure.Carriers.Avanti;

public sealed class AvantiJourneyProvider : ICarrierJourneyProvider
{
    public string CarrierCode => "AV";

    private readonly bool _shouldFail;
    private readonly bool _shouldTimeout;

    public AvantiJourneyProvider(bool shouldFail = false, bool shouldTimeout = false)
    {
        _shouldFail = shouldFail;
        _shouldTimeout = shouldTimeout;
    }

    public async Task<IReadOnlyList<CarrierJourneyResponse>> SearchJourneyAsync(Guid originStationId, Guid destinationStationId, DateTimeOffset departureFrom, DateTimeOffset departureTo, CancellationToken cancellationToken = default)
    {
        if (_shouldFail)
            throw new CarrierProviderException(CarrierCode);
        
        if (_shouldTimeout)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            throw new CarrierProviderTimeoutException(CarrierCode);
        }

        await Task.Delay(100, cancellationToken);

        var journeys = new List<CarrierJourneyResponse>
        {
            new()
            {
                CarrierCode = CarrierCode,
                ExternalJourneyId = "AV-1001",
                OriginStationId = originStationId,
                DestinationStationId = destinationStationId,
                DepartureTime = departureFrom.AddMinutes(30),
                ArrivalTime = departureFrom.AddHours(1).AddMinutes(30),
                Price = 45.00m,
                Legs =
                [
                    new CarrierJourneyLegResponse
                    {
                        OriginStationId = originStationId,
                        DestinationStationId = destinationStationId,
                        DepartureTime = departureFrom.AddMinutes(30),
                        ArrivalTime = departureFrom.AddHours(1).AddMinutes(30)
                    }
                ]
            }
        };

        return journeys.Where(journey => journey.DepartureTime >= departureFrom && journey.DepartureTime <= departureTo).ToList();
    }
}