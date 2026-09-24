using Microsoft.EntityFrameworkCore;
using Railway.Application.Repositories;
using Railway.Domain.Entities;
using Railway.Infrastructure.Data;

namespace Railway.Infrastructure.Persistence;

public sealed class JourneyRepository : IJourneyRepository
{
    private readonly RailwayDbContext _context;

    public JourneyRepository(RailwayDbContext context)
    {
        _context = context;
    }

    public async Task<Journey?> GetByIdAsync(Guid journeyId)
    {
        return await _context.Journeys.Include(journey => journey.JourneyLegs).Include(journey => journey.Fares).SingleOrDefaultAsync(journey => journey.Id == journeyId);
    }

    public async Task<IReadOnlyList<Journey>> SearchAsync(Guid originStationId, Guid destinationStaionId, DateTimeOffset departureFrom, DateTimeOffset departureTo)
    {
        return await _context.Journeys.Include(journey => journey.JourneyLegs).Include(journey => journey.Fares).Where(journey => journey.OriginStationId == originStationId && journey.DestinationStationId == destinationStaionId && journey.DepartureTime >= departureFrom && journey.DepartureTime <= departureTo).OrderBy(journey => journey.DepartureTime).ToListAsync();
    }

    public async Task UpdateAsync(Journey journey)
    {
        _context.Journeys.Update(journey);
        await Task.CompletedTask;
    }
}