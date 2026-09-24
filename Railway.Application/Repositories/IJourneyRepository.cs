using Railway.Domain.Entities;

namespace Railway.Application.Repositories;

public interface IJourneyRepository
{
    Task<Journey?> GetByIdAsync(Guid journeyId);

    Task<IReadOnlyList<Journey>> SearchAsync(Guid originStationId, Guid destinationStationId, DateTimeOffset departureFrom, DateTimeOffset departureTo);

    Task UpdateAsync(Journey journey);
}