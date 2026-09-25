using System.Net.Cache;
using Railway.Application.DTO;
using Railway.Application.Repositories;

namespace Railway.Application.Services;

public sealed class JourneyService : IJourneyService
{
    private readonly IJourneyRepository _journeyRepository;

    public JourneyService(IJourneyRepository journeyRepository)
    {
        _journeyRepository = journeyRepository;
    }

    public async Task<IReadOnlyList<JourneyResponse>> SearchAsync(JourneySearchRequest request)
    {
        if (request.OriginStationId == Guid.Empty)
            throw new ArgumentException("Origin station ID is required.");

        if (request.DestinationStationId == Guid.Empty)
            throw new ArgumentException("Destination station ID is required.");

        if (request.OriginStationId == request.DestinationStationId)
            throw new ArgumentException(
                "Origin and destination stations must be different.");

        if (request.DepartureFrom > request.DepartureTo)
            throw new ArgumentException(
                "DepartureFrom must be earlier than or equal to DepartureTo.");
                
        var journeys = await _journeyRepository.SearchAsync(request.OriginStationId, request.DestinationStationId, request.DepartureFrom, request.DepartureTo);

        return journeys.Select(MapJourney).ToList();
    }

    public async Task<JourneyResponse?> GetByIdAsync(Guid journeyId)
    {
        var journey = await _journeyRepository.GetByIdAsync(journeyId);

        if (journey is null)
            return null;
        
        return MapJourney(journey);
    }

    private static JourneyResponse MapJourney(Railway.Domain.Entities.Journey journey)
    {
        return new JourneyResponse
        {
            Id = journey.Id,
            OriginStationId = journey.OriginStationId,
            DestinationStationId = journey.DestinationStationId,
            DepartureTime = journey.DepartureTime,
            ArrivalTime = journey.ArrivalTime,
            AvailableSeats = journey.AvailableSeats,

            JourneyLegs = journey.JourneyLegs.Select(leg => new JourneyLegResponse
            {
                Id = leg.Id,
                OriginStationId = leg.OriginStationId,
                DestinationStationId = leg.DestinationStationId,
                DepartureTime = leg.DepartureTime,
                ArrivalTime = leg.ArrivalTime,
                CarrierId = leg.CarrierId
            }).ToList(),

            Fares = journey.Fares.Select(fare => new FareResponse
            {
                Id = fare.Id,
                Name = fare.Name,
                Price = fare.Price
            }).ToList()
        };
    }
}