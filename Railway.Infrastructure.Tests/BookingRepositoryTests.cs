using Microsoft.EntityFrameworkCore;
using Railway.Application.Exceptions;
using Railway.Domain.Entities;
using Railway.Infrastructure.Data;
using Railway.Infrastructure.Persistence;

namespace Railway.Infrastructure.Tests;

public class BookingRepositoryTests
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=railway;Username=railway;Password=railway_dev_password";

    [Fact]
    public async Task CreateBookingTransactionAsync_should_persist_booking_and_decrement_available_seats()
    {
        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var fareId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        await using (var setupContext = CreateContext())
        {
            var originCode = $"A{journeyId.ToString()[..8]}";
            var destinationCode = $"B{journeyId.ToString()[..8]}";

            var origin = new Station(originStationId, originCode, "Origin Station");

            var destination = new Station(destinationStationId, destinationCode, "Destination Station");

            var customer = new Customer(customerId, "Test Customer", $"test-{customerId}@example.com");

            var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 5);

            var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

            setupContext.Stations.AddRange(origin, destination);
            setupContext.Customers.AddRange(customer);
            setupContext.Journeys.AddRange(journey);
            setupContext.Fares.AddRange(fare);

            await setupContext.SaveChangesAsync();
        }

        try
        {
            await using (var context = CreateContext())
            {
                var repository = new BookingRepository(context);

                var journey = await context.Journeys.SingleAsync(journey => journey.Id == journeyId);

                var booking = new Booking(bookingId, customerId, journeyId, fareId, DateTimeOffset.UtcNow);

                journey.ReserveSeat();

                await repository.CreateBookingTransactionAsync(booking, journey);
            }

            await using (var verificationContext = CreateContext())
            {
                var savedJourney = await verificationContext.Journeys.SingleAsync(journey => journey.Id == journeyId);

                var savedBooking = await verificationContext.Bookings.SingleAsync(booking => booking.Id == bookingId);

                Assert.Equal(4, savedJourney.AvailableSeats);
                Assert.Equal(bookingId, savedBooking.Id);
                Assert.Equal(customerId, savedBooking.CustomerId);
                Assert.Equal(journeyId, savedBooking.JourneyId);
                Assert.Equal(fareId, savedBooking.FareId);
            }
        }
        finally
        {
            await CleanupAsync(journeyId, originStationId, destinationStationId, customerId);
        }
    }

    private static RailwayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RailwayDbContext>().UseNpgsql(ConnectionString).Options;

        return new RailwayDbContext(options);
    }

    private static async Task CleanupAsync(Guid journeyId, Guid originStationId, Guid destinationStationId, Guid customerId)
    {
        await using var context = CreateContext();

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

    [Fact]
    public async Task CreateBookingTransactionAsync_should_rollback_journey_update_when_booking_fails()
    {
        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var fareId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        var originCode = $"A{journeyId.ToString()[..8]}";
        var destinationCode = $"B{journeyId.ToString()[..8]}";

        try
        {
            await using (var setupContext = CreateContext())
            {
                var origin = new Station(originStationId, originCode, "Origin Station");

                var destination = new Station(destinationStationId, destinationCode, "Destination Station");

                var customer = new Customer(customerId, "Rollback Customer", $"rollback-{customerId}@example.com");

                var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 5);

                var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

                setupContext.Stations.AddRange(origin, destination);
                setupContext.Customers.Add(customer);
                setupContext.Journeys.Add(journey);
                setupContext.Fares.Add(fare);

                await setupContext.SaveChangesAsync();
            }

            await using (var context = CreateContext())
            {
                var repository = new BookingRepository(context);

                var journey = await context.Journeys.SingleAsync(journey => journey.Id == journeyId);

                var invalidFareId = Guid.NewGuid();

                var booking = new Booking(bookingId, customerId, journeyId, invalidFareId, DateTimeOffset.UtcNow);

                journey.ReserveSeat();

                await Assert.ThrowsAsync<DbUpdateException>(() => repository.CreateBookingTransactionAsync(booking, journey));
            }

            await using (var verificationContext = CreateContext())
            {
                var savedJourney = await verificationContext.Journeys.SingleAsync(journey => journey.Id == journeyId);

                var savedBooking = await verificationContext.Bookings.SingleOrDefaultAsync(booking => booking.Id == bookingId);

                Assert.Equal(5, savedJourney.AvailableSeats);
                Assert.Null(savedBooking);
            }
        }
        finally
        {
            await CleanupAsync(journeyId, originStationId, destinationStationId, customerId);
        }
    }

    [Fact]
    public async Task CreateBookingTransactionAsync_should_allow_only_one_booking_for_last_seat()
    {
        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var customerAId = Guid.NewGuid();
        var customerBId = Guid.NewGuid();
        var fareId = Guid.NewGuid();
        var bookingAId = Guid.NewGuid();
        var bookingBId = Guid.NewGuid();

        var originCode = $"A{journeyId.ToString()[..8]}";
        var destinationCode = $"B{journeyId.ToString()[..8]}";

        try
        {
            await using (var setupContext = CreateContext())
            {
                var origin = new Station( originStationId, originCode, "Origin Station");

                var destination = new Station(destinationStationId, destinationCode, "Destination Station");

                var customerA = new Customer(customerAId, "Customer A", $"customer-a-{customerAId}@example.com");

                var customerB = new Customer(customerBId, "Customer B", $"customer-b-{customerBId}@example.com");

                var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 1);

                var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

                setupContext.Stations.AddRange(origin, destination);
                setupContext.Customers.AddRange(customerA, customerB);
                setupContext.Journeys.Add(journey);
                setupContext.Fares.Add(fare);

                await setupContext.SaveChangesAsync();
            }

            await using var contextA = CreateContext();
            await using var contextB = CreateContext();

            var repositoryA = new BookingRepository(contextA);
            var repositoryB = new BookingRepository(contextB);

            var journeyA = await contextA.Journeys.SingleAsync(journey => journey.Id == journeyId);

            var journeyB = await contextB.Journeys.SingleAsync(journey => journey.Id == journeyId);

            Assert.Equal(1, journeyA.AvailableSeats);
            Assert.Equal(1, journeyB.AvailableSeats);

            journeyA.ReserveSeat();
            journeyB.ReserveSeat();

            var bookingA = new Booking(bookingAId, customerAId, journeyId, fareId, DateTimeOffset.UtcNow);

            var bookingB = new Booking(bookingBId, customerBId, journeyId, fareId, DateTimeOffset.UtcNow);

            await repositoryA.CreateBookingTransactionAsync(bookingA, journeyA);

            //await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => repositoryB.CreateBookingTransactionAsync(bookingB, journeyB));
            await Assert.ThrowsAsync<BookingConflictException>(() => repositoryB.CreateBookingTransactionAsync(bookingB, journeyB));

            await using (var verificationContext = CreateContext())
            {
                var savedJourney = await verificationContext.Journeys.SingleAsync(journey => journey.Id == journeyId);

                var savedBookings = await verificationContext.Bookings.Where(booking => booking.JourneyId == journeyId).ToListAsync();

                Assert.Equal(0, savedJourney.AvailableSeats);
                Assert.Single(savedBookings);
                Assert.Equal(bookingAId, savedBookings[0].Id);
            }
        }
        finally
        {
            await CleanupAsync(journeyId, originStationId, destinationStationId, customerAId);

            await using var cleanupContext = CreateContext();

            var customerB = await cleanupContext.Customers.SingleOrDefaultAsync(customer => customer.Id == customerBId);

            if (customerB is not null)
                cleanupContext.Customers.Remove(customerB);

            await cleanupContext.SaveChangesAsync();
        }
    }
}