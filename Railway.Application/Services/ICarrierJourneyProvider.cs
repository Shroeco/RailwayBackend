using Railway.Application.DTO;

namespace Railway.Application.Services;

public interface ICarrierJourneyProvider
{
    string CarrierCode { get; }

    Task<IReadOnlyList<CarrierJourneyResponse>> SearchJourneyAsync(Guid originStationId, Guid destinationStationId, DateTimeOffset departureFrom, DateTimeOffset departureTo, CancellationToken cancellationToken = default);
}