using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Railway.Application.DTO;
using Railway.Domain.Entities;
using Railway.Domain.Enums;
using Railway.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Railway.Application.Services;

namespace Railway.Api.Tests;

public sealed class BookingEndpointsTests
{
    [Fact]
    public async Task Create_should_persist_booking_and_return_created()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var fareId = Guid.NewGuid();

        try
        {
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var origin = new Station(originStationId, $"A{journeyId.ToString()[..8]}", "Booking Test Origin");

                var destination = new Station(destinationStationId, $"B{journeyId.ToString()[..8]}", "Booking Test Destination");

                var customer = new Customer(customerId, "Booking Test Customer", $"booking-{customerId}@example.com");

                var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow.AddHours(3), 5);

                var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

                context.Stations.AddRange(origin, destination);
                context.Customers.Add(customer);
                context.Journeys.Add(journey);
                context.Fares.Add(fare);

                await context.SaveChangesAsync();
            }

            var request = new CreateBookingRequest
            {
                CustomerId = customerId,
                JourneyId = journeyId,
                FareId = fareId
            };

            var response = await client.PostAsJsonAsync("/api/bookings", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var bookingResponse = await response.Content.ReadFromJsonAsync<BookingResponse>();

            Assert.NotNull(bookingResponse);

            Assert.NotEqual(Guid.Empty, bookingResponse.Id);
            Assert.Equal(customerId, bookingResponse.CustomerId);
            Assert.Equal(journeyId, bookingResponse.JourneyId);
            Assert.Equal(fareId, bookingResponse.FareId);
            Assert.Equal("Confirmed", bookingResponse.Status);

            Assert.NotNull(response.Headers.Location);

            Assert.Equal($"/api/bookings/{bookingResponse.Id}", response.Headers.Location.AbsolutePath);

            using (var verificationScope = factory.Services.CreateScope())
            {
                var context = verificationScope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var savedBooking = await context.Bookings.SingleAsync(booking => booking.Id == bookingResponse.Id);

                var savedJourney = await context.Journeys.SingleAsync(journey => journey.Id == journeyId);

                Assert.Equal(customerId, savedBooking.CustomerId);
                Assert.Equal(journeyId, savedBooking.JourneyId);
                Assert.Equal(fareId, savedBooking.FareId);

                Assert.Equal(4, savedJourney.AvailableSeats);
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

            var customer = await context.Customers.SingleOrDefaultAsync(customer => customer.Id == customerId);

            if (customer is not null)
                context.Customers.Remove(customer);

            var stations = await context.Stations.Where(station => station.Id == originStationId || station.Id == destinationStationId).ToListAsync();

            context.Stations.RemoveRange(stations);

            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task GetById_should_return_existing_booking()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var fareId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        try
        {
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var origin = new Station(originStationId, $"A{journeyId.ToString()[..8]}", "Get Booking Origin");

                var destination = new Station(destinationStationId, $"B{journeyId.ToString()[..8]}", "Get Booking Destination");

                var customer = new Customer(customerId, "Get Booking Customer", $"get-booking-{customerId}@example.com");

                var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow.AddHours(3), 5);

                var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

                var booking = new Booking(bookingId, customerId, journeyId, fareId, DateTimeOffset.UtcNow);

                context.Stations.AddRange(origin, destination);
                context.Customers.Add(customer);
                context.Journeys.Add(journey);
                context.Fares.Add(fare);
                context.Bookings.Add(booking);

                await context.SaveChangesAsync();
            }

            var response = await client.GetAsync($"/api/bookings/{bookingId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var bookingResponse = await response.Content.ReadFromJsonAsync<BookingResponse>();

            Assert.NotNull(bookingResponse);

            Assert.Equal(bookingId, bookingResponse.Id);
            Assert.Equal(customerId, bookingResponse.CustomerId);
            Assert.Equal(journeyId, bookingResponse.JourneyId);
            Assert.Equal(fareId, bookingResponse.FareId);
            Assert.Equal("Confirmed", bookingResponse.Status);
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

            var customer = await context.Customers.SingleOrDefaultAsync(customer => customer.Id == customerId);

            if (customer is not null)
                context.Customers.Remove(customer);

            var stations = await context.Stations.Where(station => station.Id == originStationId || station.Id == destinationStationId).ToListAsync();

            context.Stations.RemoveRange(stations);

            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Cancel_should_persist_cancellation_and_return_no_content()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var fareId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        try
        {
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var origin = new Station(originStationId, $"A{journeyId.ToString()[..8]}", "Cancel Booking Origin");

                var destination = new Station(destinationStationId, $"B{journeyId.ToString()[..8]}", "Cancel Booking Destination");

                var customer = new Customer(customerId, "Cancel Booking Customer", $"cancel-booking-{customerId}@example.com");

                var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow.AddHours(3), 5);

                var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

                var booking = new Booking(bookingId, customerId, journeyId, fareId, DateTimeOffset.UtcNow);

                context.Stations.AddRange(origin, destination);
                context.Customers.Add(customer);
                context.Journeys.Add(journey);
                context.Fares.Add(fare);
                context.Bookings.Add(booking);

                await context.SaveChangesAsync();
            }

            var response = await client.DeleteAsync($"/api/bookings/{bookingId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using (var verificationScope = factory.Services.CreateScope())
            {
                var context = verificationScope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var savedBooking = await context.Bookings.SingleAsync(booking => booking.Id == bookingId);

                Assert.Equal(Railway.Domain.Enums.BookingStatus.Cancelled, savedBooking.Status);
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

            var customer = await context.Customers.SingleOrDefaultAsync(customer => customer.Id == customerId);

            if (customer is not null)
                context.Customers.Remove(customer);

            var stations = await context.Stations.Where(station => station.Id == originStationId || station.Id == destinationStationId).ToListAsync();

            context.Stations.RemoveRange(stations);

            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task GetById_should_return_not_found_when_booking_does_not_exist()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var bookingId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/bookings/{bookingId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_should_return_conflict_when_no_seats_are_available()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var fareId = Guid.NewGuid();

        try
        {
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var origin = new Station(originStationId, $"A{journeyId.ToString()[..8]}", "Conflict Test Origin");

                var destination = new Station(destinationStationId, $"B{journeyId.ToString()[..8]}", "Conflict Test Destination");

                var customer = new Customer(customerId, "Conflict Test Customer", $"conflict-{customerId}@example.com");

                var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow.AddHours(3), 1);

                journey.ReserveSeat();

                var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

                context.Stations.AddRange(origin, destination);
                context.Customers.Add(customer);
                context.Journeys.Add(journey);
                context.Fares.Add(fare);

                await context.SaveChangesAsync();
            }

            var request = new CreateBookingRequest
            {
                CustomerId = customerId,
                JourneyId = journeyId,
                FareId = fareId
            };

            var response = await client.PostAsJsonAsync("/api/bookings", request);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            using var verificationScope = factory.Services.CreateScope();

            var verificationContext = verificationScope.ServiceProvider.GetRequiredService<RailwayDbContext>();

            var bookingExists = await verificationContext.Bookings.AnyAsync(booking => booking.JourneyId == journeyId);

            var savedJourney = await verificationContext.Journeys.SingleAsync(journey => journey.Id == journeyId);

            Assert.False(bookingExists);
            Assert.Equal(0, savedJourney.AvailableSeats);
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

            var customer = await context.Customers.SingleOrDefaultAsync(customer => customer.Id == customerId);

            if (customer is not null)
                context.Customers.Remove(customer);

            var stations = await context.Stations.Where(station => station.Id == originStationId || station.Id == destinationStationId).ToListAsync();

            context.Stations.RemoveRange(stations);

            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Cancel_should_return_bad_request_when_booking_is_cancelled_twice()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var journeyId = Guid.NewGuid();
        var fareId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        try
        {
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var origin = new Station( originStationId, $"A{journeyId.ToString()[..8]}", "Double Cancellation Origin");

                var destination = new Station(destinationStationId, $"B{journeyId.ToString()[..8]}", "Double Cancellation Destination");

                var customer = new Customer(customerId, "Double Cancellation Customer", $"double-cancellation-{customerId}@example.com");

                var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow.AddHours(3), 1);

                var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

                var booking = new Booking(bookingId, customerId, journeyId, fareId, DateTimeOffset.UtcNow);

                context.Stations.AddRange(origin, destination);
                context.Customers.Add(customer);
                context.Journeys.Add(journey);
                context.Fares.Add(fare);
                context.Bookings.Add(booking);

                await context.SaveChangesAsync();
            }

            var firstResponse = await client.DeleteAsync($"/api/bookings/{bookingId}");

            Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

            var secondResponse = await client.DeleteAsync($"/api/bookings/{bookingId}");

            Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);

            using (var verificationScope = factory.Services.CreateScope())
            {
                var context = verificationScope.ServiceProvider.GetRequiredService<RailwayDbContext>();

                var savedBooking = await context.Bookings.SingleAsync(booking => booking.Id == bookingId);

                Assert.Equal(BookingStatus.Cancelled, savedBooking.Status);
            }
        }
        finally
        {
            using var scope = factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

            var bookings = await context.Bookings.Where(booking => booking.Id == bookingId).ToListAsync();

            context.Bookings.RemoveRange(bookings);

            var fares = await context.Fares.Where(fare => fare.JourneyId == journeyId).ToListAsync();

            context.Fares.RemoveRange(fares);

            var journey = await context.Journeys.SingleOrDefaultAsync(journey => journey.Id == journeyId);

            if (journey is not null)
                context.Journeys.Remove(journey);

            var customer = await context.Customers.SingleOrDefaultAsync(customer => customer.Id == customerId);

            if (customer is not null)
                context.Customers.Remove(customer);

            var stations = await context.Stations.Where(station => station.Id == originStationId || station.Id == destinationStationId).ToListAsync();

            context.Stations.RemoveRange(stations);

            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Create_should_return_problem_details_with_correlation_id_for_invalid_request()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        const string correlationId = "booking-error-test";

        client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

        var request = new CreateBookingRequest
        {
            CustomerId = Guid.Empty,
            JourneyId = Guid.Empty,
            FareId = Guid.Empty
        };

        var response = await client.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var correlationHeaderValues));

        Assert.Equal(correlationId, Assert.Single(correlationHeaderValues));

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);

        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);

        Assert.Equal("Bad Request", problemDetails.Title);

        Assert.Equal("Customer ID is required.", problemDetails.Detail);

        Assert.True(problemDetails.Extensions.TryGetValue("correlationId", out var correlationIdExtension));

        Assert.NotNull(correlationIdExtension);

        Assert.Equal(correlationId, correlationIdExtension.ToString());
    }

    [Fact]
    public async Task Create_should_return_safe_problem_details_for_unexpected_exception()
    {
        await using var baseFactory = new CustomWebApplicationFactory();

        await using var factory = baseFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IBookingService>();
                services.AddScoped<IBookingService, ThrowingBookingService>();
            });
        });

        using var client = factory.CreateClient();

        const string correlationId = "unexpected-error-test";

        client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

        var request = new CreateBookingRequest
        {
            CustomerId = Guid.NewGuid(),
            JourneyId = Guid.NewGuid(),
            FareId = Guid.NewGuid()
        };

        var response = await client.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var correlationHeaderValues));

        Assert.Equal(correlationId, Assert.Single(correlationHeaderValues));

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);

        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);

        Assert.Equal("Internal Server Error", problemDetails.Title);

        Assert.Equal("An unexpected error occurred.", problemDetails.Detail);

        Assert.True(problemDetails.Extensions.TryGetValue("correlationId", out var correlationIdExtension));

        Assert.NotNull(correlationIdExtension);

        Assert.Equal(correlationId, correlationIdExtension.ToString());

        Assert.DoesNotContain("Test unexpected exception", await response.Content.ReadAsStringAsync());
    }

    private sealed class ThrowingBookingService : IBookingService
    {
        public Task<BookingResponse> CreateAsync(CreateBookingRequest request)
        {
            throw new Exception("Test unexpected exception");
        }

        public Task<BookingResponse?> GetByIdAsync(Guid bookingId)
        {
            throw new NotImplementedException();
        }
    }

}