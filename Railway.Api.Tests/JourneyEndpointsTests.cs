using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Railway.Application.DTO;
using Railway.Domain.Entities;
using Railway.Infrastructure.Data;

namespace Railway.Api.Tests;

public sealed class JourneyEndpointsTests
{
    [Fact]
    public async Task Search_should_return_matching_journey()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();

        var departureTime = DateTimeOffset.UtcNow.AddHours(2);
        var arrivalTime = departureTime.AddHours(1);

        try
        {
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var origin = new Station(originStationId, $"A{journeyId.ToString()[..8]}", "API Test Origin");

                var destination = new Station(destinationStationId, $"B{journeyId.ToString()[..8]}", "API Test Destination");

                var journey = new Journey(journeyId, originStationId, destinationStationId, departureTime, arrivalTime, 10);

                context.Stations.AddRange(origin, destination);
                context.Journeys.Add(journey);

                await context.SaveChangesAsync();
            }

            var departureFrom = departureTime.AddMinutes(-30);
            var departureTo = departureTime.AddMinutes(30);

            var url = $"/api/journeys/search" + $"?OriginStationId={originStationId}" + $"&DestinationStationId={destinationStationId}" + $"&DepartureFrom={Uri.EscapeDataString(departureFrom.ToString("O"))}" + $"&DepartureTo={Uri.EscapeDataString(departureTo.ToString("O"))}";

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var journeys = await response.Content.ReadFromJsonAsync<List<JourneyResponse>>();

            Assert.NotNull(journeys);

            var journeyResponse = Assert.Single(journeys, journey => journey.Id == journeyId);

            Assert.Equal(originStationId, journeyResponse.OriginStationId);
            Assert.Equal(destinationStationId, journeyResponse.DestinationStationId);
            Assert.Equal(10, journeyResponse.AvailableSeats);
        }
        finally
        {
            using var scope = factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

            var journey = await context.Journeys.SingleOrDefaultAsync(journey => journey.Id == journeyId);

            if (journey is not null)
                context.Journeys.Remove(journey);

            var stations = await context.Stations.Where(station => station.Id == originStationId || station.Id == destinationStationId).ToListAsync();

            context.Stations.RemoveRange(stations);

            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task GetById_should_return_existing_journey()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();

        var departureTime = DateTimeOffset.UtcNow.AddHours(2);
        var arrivalTime = departureTime.AddHours(1);

        try
        {
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var origin = new Station(originStationId, $"A{journeyId.ToString()[..8]}", "Journey Details Origin");

                var destination = new Station(destinationStationId, $"B{journeyId.ToString()[..8]}", "Journey Details Destination");

                var journey = new Journey(journeyId, originStationId, destinationStationId, departureTime, arrivalTime, 10);

                context.Stations.AddRange(origin, destination);
                context.Journeys.Add(journey);

                await context.SaveChangesAsync();
            }

            var response = await client.GetAsync($"/api/journeys/{journeyId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var journeyResponse = await response.Content.ReadFromJsonAsync<JourneyResponse>();

            Assert.NotNull(journeyResponse);

            Assert.Equal(journeyId, journeyResponse.Id);
            Assert.Equal(originStationId, journeyResponse.OriginStationId);
            Assert.Equal(destinationStationId, journeyResponse.DestinationStationId);
            Assert.Equal(departureTime, journeyResponse.DepartureTime, TimeSpan.FromMilliseconds(1));
            Assert.Equal(arrivalTime, journeyResponse.ArrivalTime, TimeSpan.FromMilliseconds(1));
            Assert.Equal(10, journeyResponse.AvailableSeats);
        }
        finally
        {
            using var scope = factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

            var journey = await context.Journeys.SingleOrDefaultAsync(journey => journey.Id == journeyId);

            if (journey is not null)
                context.Journeys.Remove(journey);

            var stations = await context.Stations.Where(station => station.Id == originStationId || station.Id == destinationStationId).ToListAsync();

            context.Stations.RemoveRange(stations);

            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Search_should_return_bad_request_when_origin_and_destination_are_same()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var stationId = Guid.NewGuid();
        var departureFrom = DateTimeOffset.UtcNow.AddHours(1);
        var departureTo = departureFrom.AddHours(2);

        var url =$"/api/journeys/search" + $"?OriginStationId={stationId}" + $"&DestinationStationId={stationId}" + $"&DepartureFrom={Uri.EscapeDataString(departureFrom.ToString("O"))}" + $"&DepartureTo={Uri.EscapeDataString(departureTo.ToString("O"))}";

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);

        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("Bad Request", problemDetails.Title);
        Assert.Equal("Origin and destination stations must be different.", problemDetails.Detail);
    }

    [Fact]
    public async Task GetById_should_return_not_found_when_journey_does_not_exist()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var journeyId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/journeys/{journeyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

}