using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Railway.Domain.Entities;
using Railway.Infrastructure.Data;
using System.Net.Http.Json;
using Railway.Application.DTO;

namespace Railway.Api.Tests;

public sealed class BookingConcurrencyTests
{
    [Fact]
    public async Task Two_concurrent_booking_requests_should_allow_only_one_booking_for_last_seat()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var firstClient = factory.CreateClient();
        using var secondClient = factory.CreateClient();

        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var firstCustomerId = Guid.NewGuid();
        var secondCustomerId = Guid.NewGuid();
        var fareId = Guid.NewGuid();

        try
        {
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var origin = new Station(originStationId, $"A{journeyId.ToString()[..8]}", "Concurrency Origin");

                var destination = new Station(destinationStationId, $"B{journeyId.ToString()[..8]}", "Concurrency Destination");

                var firstCustomer = new Customer(firstCustomerId, "Concurrency Customer One", $"concurrency-one-{firstCustomerId}@example.com");

                var secondCustomer = new Customer(secondCustomerId, "Concurrency Customer Two", $"concurrency-two-{secondCustomerId}@example.com");

                var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow.AddHours(3), 1);

                var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

                context.Stations.AddRange(origin, destination);
                context.Customers.AddRange(firstCustomer, secondCustomer);
                context.Journeys.Add(journey);
                context.Fares.Add(fare);

                await context.SaveChangesAsync();
            }

            using (var verificationScope = factory.Services.CreateScope())
            {
                var context = verificationScope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var savedJourney = await context.Journeys.SingleAsync(journey => journey.Id == journeyId);

                Assert.Equal(1, savedJourney.AvailableSeats);
            }

            var firstRequest = new CreateBookingRequest
            {
                CustomerId = firstCustomerId,
                JourneyId = journeyId,
                FareId = fareId
            };

            var secondRequest = new CreateBookingRequest
            {
                CustomerId = secondCustomerId,
                JourneyId = journeyId,
                FareId = fareId
            };

            var firstBookingTask = firstClient.PostAsJsonAsync("/api/bookings", firstRequest);

            var secondBookingTask = secondClient.PostAsJsonAsync("/api/bookings", secondRequest);

            await Task.WhenAll(firstBookingTask, secondBookingTask);

            var firstResponse = await firstBookingTask;
            var secondResponse = await secondBookingTask;

            var responses = new[]
            {
                firstResponse,
                secondResponse
            };

            var successfulResponses = responses.Where(response => response.StatusCode == System.Net.HttpStatusCode.Created).ToList();

            Assert.Single(successfulResponses);

            var conflictResponses = responses.Where(response => response.StatusCode == System.Net.HttpStatusCode.Conflict).ToList();

            Assert.Single(conflictResponses);

            var successfulBooking = await successfulResponses.Single().Content.ReadFromJsonAsync<BookingResponse>();

            Assert.NotNull(successfulBooking);

            using (var verificationScope = factory.Services.CreateScope())
            {
                var context = verificationScope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var savedJourney = await context.Journeys.SingleAsync(journey => journey.Id == journeyId);

                Assert.Equal(0, savedJourney.AvailableSeats);
                Assert.True(savedJourney.AvailableSeats >= 0);

                var savedBookings = await context.Bookings.Where(booking => booking.JourneyId == journeyId).ToListAsync();

                Assert.Single(savedBookings);

                var savedBooking = savedBookings.Single();

                Assert.Equal(successfulBooking.Id, savedBooking.Id);
                Assert.Equal(successfulBooking.CustomerId, savedBooking.CustomerId);
                Assert.Equal(journeyId, savedBooking.JourneyId);
                Assert.Equal(fareId, savedBooking.FareId);

                Assert.True(savedBooking.CustomerId == firstCustomerId || savedBooking.CustomerId == secondCustomerId);
            }
        }
        finally
        {
            using var scope = factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

            var bookings = await context.Bookings.Where(booking => booking.JourneyId == journeyId).ToListAsync();

            context.Bookings.RemoveRange(bookings);

            var fares = await context.Fares.Where(fare => fare.JourneyId == journeyId).ToListAsync();

            context.Fares.RemoveRange(fares);

            var journey = await context.Journeys.SingleOrDefaultAsync(journey => journey.Id == journeyId);

            if (journey is not null)
                context.Journeys.Remove(journey);

            var customers = await context.Customers.Where(customer => customer.Id == firstCustomerId || customer.Id == secondCustomerId).ToListAsync();

            context.Customers.RemoveRange(customers);

            var stations = await context.Stations.Where(station => station.Id == originStationId || station.Id == destinationStationId).ToListAsync();

            context.Stations.RemoveRange(stations);

            await context.SaveChangesAsync();
        }
    }
}