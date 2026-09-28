using Railway.Application.DTO;
using Railway.Application.Repositories;
using Railway.Application.Services;
using Railway.Domain.Entities;

namespace Railway.Application.Tests;

public class JourneyServiceTests
{
    [Fact]
    public async Task SearchAsync_should_return_matching_journeys_when_request_is_valid()
    {
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var journeyId = Guid.NewGuid();

        var departureFrom = DateTimeOffset.UtcNow.AddHours(1);
        var departureTo = DateTimeOffset.UtcNow.AddHours(4);

        var journey = new Journey(journeyId, originStationId, destinationStationId, departureFrom.AddHours(1), departureFrom.AddHours(2), 10);

        var journeyRepository = new FakeJourneyRepository(journey);
        var service = new JourneyService(journeyRepository);

        var request = new JourneySearchRequest
        {
            OriginStationId = originStationId,
            DestinationStationId = destinationStationId,
            DepartureFrom = departureFrom,
            DepartureTo = departureTo
        };

        var response = await service.SearchAsync(request);

        var result = Assert.Single(response);

        Assert.Equal(journeyId, result.Id);
        Assert.Equal(originStationId, result.OriginStationId);
        Assert.Equal(destinationStationId, result.DestinationStationId);
        Assert.Equal(journey.DepartureTime, result.DepartureTime);
        Assert.Equal(journey.ArrivalTime, result.ArrivalTime);
        Assert.Equal(10, result.AvailableSeats);

        Assert.Equal(originStationId, journeyRepository.SearchOriginStationId);
        Assert.Equal(destinationStationId, journeyRepository.SearchDestinationStationId);
        Assert.Equal(departureFrom, journeyRepository.SearchDepartureFrom);
        Assert.Equal(departureTo, journeyRepository.SearchDepartureTo);
    }

    [Fact]
    public async Task SearchAsync_should_throw_when_origin_station_id_is_empty()
    {
        var journeyRepository = new FakeJourneyRepository(null!);
        var service = new JourneyService(journeyRepository);

        var request = new JourneySearchRequest
        {
            OriginStationId = Guid.Empty,
            DestinationStationId = Guid.NewGuid(),
            DepartureFrom = DateTimeOffset.UtcNow,
            DepartureTo = DateTimeOffset.UtcNow.AddHours(2)
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.SearchAsync(request));

        Assert.Equal("Origin station ID is required.", exception.Message);
    }

    [Fact]
    public async Task SearchAsync_should_throw_when_destination_station_id_is_empty()
    {
        var journeyRepository = new FakeJourneyRepository(null!);
        var service = new JourneyService(journeyRepository);

        var request = new JourneySearchRequest
        {
            OriginStationId = Guid.NewGuid(),
            DestinationStationId = Guid.Empty,
            DepartureFrom = DateTimeOffset.UtcNow,
            DepartureTo = DateTimeOffset.UtcNow.AddHours(2)
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.SearchAsync(request));

        Assert.Equal("Destination station ID is required.", exception.Message);
    }

    [Fact]
    public async Task SearchAsync_should_throw_when_origin_and_destination_are_the_same()
    {
        var stationId = Guid.NewGuid();

        var journeyRepository = new FakeJourneyRepository(null!);
        var service = new JourneyService(journeyRepository);

        var request = new JourneySearchRequest
        {
            OriginStationId = stationId,
            DestinationStationId = stationId,
            DepartureFrom = DateTimeOffset.UtcNow,
            DepartureTo = DateTimeOffset.UtcNow.AddHours(2)
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.SearchAsync(request));

        Assert.Equal("Origin and destination stations must be different.", exception.Message);
    }

    [Fact]
    public async Task SearchAsync_should_throw_when_departure_from_is_after_departure_to()
    {
        var departureFrom = DateTimeOffset.UtcNow.AddHours(2);
        var departureTo = DateTimeOffset.UtcNow;

        var journeyRepository = new FakeJourneyRepository(null!);
        var service = new JourneyService(journeyRepository);

        var request = new JourneySearchRequest
        {
            OriginStationId = Guid.NewGuid(),
            DestinationStationId = Guid.NewGuid(),
            DepartureFrom = departureFrom,
            DepartureTo = departureTo
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.SearchAsync(request));

        Assert.Equal("DepartureFrom must be earlier than or equal to DepartureTo.", exception.Message);
    }

    private sealed class FakeJourneyRepository : IJourneyRepository
    {
        private readonly Journey _journey;

        public Guid? SearchOriginStationId { get; private set; }
        public Guid? SearchDestinationStationId { get; private set; }
        public DateTimeOffset? SearchDepartureFrom { get; private set; }
        public DateTimeOffset? SearchDepartureTo { get; private set; }

        public FakeJourneyRepository(Journey journey)
        {
            _journey = journey;
        }

        public Task<Journey?> GetByIdAsync(Guid journeyId)
        {
            return Task.FromResult<Journey?>(null);
        }

        public Task<IReadOnlyList<Journey>> SearchAsync(Guid originStationId, Guid destinationStationId, DateTimeOffset departureFrom, DateTimeOffset departureTo)
        {
            SearchOriginStationId = originStationId;
            SearchDestinationStationId = destinationStationId;
            SearchDepartureFrom = departureFrom;
            SearchDepartureTo = departureTo;

            return Task.FromResult<IReadOnlyList<Journey>>(new[] { _journey });
        }

        public Task UpdateAsync(Journey journey)
        {
            return Task.CompletedTask;
        }
    }
}