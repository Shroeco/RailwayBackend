using Microsoft.EntityFrameworkCore;
using Railway.Application.Exceptions;
using Railway.Domain.Entities;
using Railway.Infrastructure.Data;
using Railway.Infrastructure.Persistence;

namespace Railway.Infrastructure.Tests;

public class BookingRepositoryTests : IClassFixture<PostgresTestFixture>
{
    // private const string ConnectionString = "Host=localhost;Port=5432;Database=railway;Username=railway;Password=railway_dev_password";

    // private static readonly PostgresTestFixture Fixture = new();

    private readonly PostgresTestFixture _fixture;

    public BookingRepositoryTests(PostgresTestFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

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

    // private static RailwayDbContext CreateContext()
    // {
    //     var options = new DbContextOptionsBuilder<RailwayDbContext>().UseNpgsql(ConnectionString).Options;

    //     return new RailwayDbContext(options);
    // }
    private RailwayDbContext CreateContext()
    {
        return _fixture.CreateContext();
    }

    private async Task CleanupAsync(Guid journeyId, Guid originStationId, Guid destinationStationId, Guid? customerId = null)
    {
        await using var context = CreateContext();

        var bookings = await context.Bookings.Where(booking => booking.JourneyId == journeyId).ToListAsync();

        context.Bookings.RemoveRange(bookings);

        var fares = await context.Fares.Where(fare => fare.JourneyId == journeyId).ToListAsync();

        context.Fares.RemoveRange(fares);

        var journey = await context.Journeys.SingleOrDefaultAsync(journey => journey.Id == journeyId);

        if (journey is not null)
            context.Journeys.Remove(journey);
        
        // var customer = await context.Customers.SingleOrDefaultAsync(customer => customer.Id == customerId);

        // if (customer is not null)
        //     context.Customers.Remove(customer);

        if (customerId.HasValue)
        {
            var customer = await context.Customers.SingleOrDefaultAsync(customer => customer.Id == customerId.Value);

            if (customer is not null)
                context.Customers.Remove(customer);
        }

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

    [Fact]
    public async Task Test_database_should_be_available()
    {
        await _fixture.EnsureDatabaseAvailableAsync();
    }

    [Fact]
    public async Task PostgreSQL_should_persist_and_read_back_a_journey()
    {
        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();

        await using (var setupContext = CreateContext())
        {
            var originCode = $"A{journeyId.ToString()[..8]}";
            var destinationCode = $"B{journeyId.ToString()[..8]}";

            var origin = new Station(originStationId, originCode, "Origin Station");

            var destination = new Station(destinationStationId, destinationCode, "Destination Station");

            var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 10);

            setupContext.Stations.AddRange(origin, destination);
            setupContext.Journeys.Add(journey);

            await setupContext.SaveChangesAsync();
        }

        try
        {
            await using var verificationContext = CreateContext();

            var savedJourney = await verificationContext.Journeys.SingleAsync(journey => journey.Id == journeyId);

            Assert.Equal(journeyId, savedJourney.Id);
            Assert.Equal(originStationId, savedJourney.OriginStationId);
            Assert.Equal(destinationStationId, savedJourney.DestinationStationId);
            Assert.Equal(10, savedJourney.AvailableSeats);
        }
        finally
        {
            await CleanupAsync(journeyId, originStationId, destinationStationId);
        }
    }

    [Fact]
    public async Task CleanupAsync_should_only_remove_test_owned_data()
    {
        var journeyAId = Guid.NewGuid();
        var originAId = Guid.NewGuid();
        var destinationAId = Guid.NewGuid();

        var journeyBId = Guid.NewGuid();
        var originBId = Guid.NewGuid();
        var destinationBId = Guid.NewGuid();

        await using (var setupContext = CreateContext())
        {
            var originA = new Station(originAId, $"A{journeyAId.ToString()[..8]}", "Origin A");

            var destinationA = new Station(destinationAId, $"B{journeyAId.ToString()[..8]}", "Destination A");

            var originB = new Station(originBId, $"A{journeyBId.ToString()[..8]}", "Origin B");

            var destinationB = new Station(destinationBId, $"B{journeyBId.ToString()[..8]}", "Destination B");

            var journeyA = new Journey(journeyAId, originAId, destinationAId, DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 5);

            var journeyB = new Journey(journeyBId, originBId, destinationBId, DateTimeOffset.UtcNow.AddHours(3), DateTimeOffset.UtcNow.AddHours(4), 5);

            setupContext.Stations.AddRange(originA, destinationA, originB, destinationB);

            setupContext.Journeys.AddRange(journeyA, journeyB);

            await setupContext.SaveChangesAsync();
        }

        try
        {
            await CleanupAsync(journeyAId, originAId, destinationAId);

            await using var verificationContext = CreateContext();

            var journeyAExists = await verificationContext.Journeys.AnyAsync(journey => journey.Id == journeyAId);

            var journeyBExists = await verificationContext.Journeys.AnyAsync(journey => journey.Id == journeyBId);

            var originBExists = await verificationContext.Stations.AnyAsync(station => station.Id == originBId);

            var destinationBExists = await verificationContext.Stations.AnyAsync(station => station.Id == destinationBId);

            Assert.False(journeyAExists);
            Assert.True(journeyBExists);
            Assert.True(originBExists);
            Assert.True(destinationBExists);
        }
        finally
        {
            await CleanupAsync(journeyBId, originBId, destinationBId);
        }
    }

    [Fact]
    public async Task GetByIdAsync_should_return_existing_booking()
    {
        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var fareId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        try
        {
            await using (var setupContext = CreateContext())
            {
                var origin = new Station(originStationId, $"A{journeyId.ToString()[..8]}", "Origin Station");

                var destination = new Station(destinationStationId, $"B{journeyId.ToString()[..8]}", "Destination Station");

                var customer = new Customer(customerId, "Retrieval Customer", $"retrieval-{customerId}@example.com");

                var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 5);

                var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

                var booking = new Booking(bookingId, customerId, journeyId, fareId, DateTimeOffset.UtcNow);

                setupContext.Stations.AddRange(origin, destination);
                setupContext.Customers.Add(customer);
                setupContext.Journeys.Add(journey);
                setupContext.Fares.Add(fare);
                setupContext.Bookings.Add(booking);

                await setupContext.SaveChangesAsync();
            }

            await using var context = CreateContext();

            var repository = new BookingRepository(context);

            var Retrievedooking = await repository.GetByIdAsync(bookingId);

            Assert.NotNull(Retrievedooking);
            Assert.Equal(bookingId, Retrievedooking.Id);
            Assert.Equal(customerId, Retrievedooking.CustomerId);
            Assert.Equal(journeyId, Retrievedooking.JourneyId);
            Assert.Equal(fareId, Retrievedooking.FareId);
        }
        finally
        {
            await CleanupAsync(journeyId, originStationId, destinationStationId, customerId);
        }
    }

    [Fact]
    public async Task UpdateAsync_should_persist_booking_cancellation()
    {
        var journeyId = Guid.NewGuid();
        var originStationId = Guid.NewGuid();
        var destinationStationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var fareId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        try
        {
            await using (var setupContext = CreateContext())
            {
                var origin = new Station(originStationId, $"A{journeyId.ToString()[..8]}", "Origin Station");

                var destination = new Station(destinationStationId, $"B{journeyId.ToString()[..8]}", "Destination Station");

                var customer = new Customer(customerId, "Cancellation Customer", $"cancellation-{customerId}@example.com");

                var journey = new Journey(journeyId, originStationId, destinationStationId, DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 5);

                var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

                var booking = new Booking(bookingId, customerId, journeyId, fareId, DateTimeOffset.UtcNow);

                setupContext.Stations.AddRange(origin, destination);
                setupContext.Customers.Add(customer);
                setupContext.Journeys.Add(journey);
                setupContext.Fares.Add(fare);
                setupContext.Bookings.Add(booking);

                await setupContext.SaveChangesAsync();
            }

            await using (var context = CreateContext())
            {
                var repository = new BookingRepository(context);

                var booking = await repository.GetByIdAsync(bookingId);

                Assert.NotNull(booking);

                booking.Cancel();

                await repository.UpdateAsync(booking);
            }

            await using (var verificationContext = CreateContext())
            {
                var savedBooking = await verificationContext.Bookings.SingleAsync(booking => booking.Id == bookingId);

                Assert.Equal(Railway.Domain.Enums.BookingStatus.Cancelled, savedBooking.Status);
            }
        }
        finally
        {
            await CleanupAsync(journeyId, originStationId, destinationStationId, customerId);
        }
    }

    [Fact]
    public async Task Database_should_reject_duplicate_customer_email()
    {
        var firstCustomerId = Guid.NewGuid();
        var secondCustomerId = Guid.NewGuid();
        var email = $"duplicate-{Guid.NewGuid()}@example.com";

        try
        {
            await using var context = CreateContext();

            var firstCustomer = new Customer(firstCustomerId, "First Customer", email);

            var secondCustomer = new Customer(secondCustomerId, "Second Customer", email);

            context.Customers.Add(firstCustomer);

            await context.SaveChangesAsync();

            context.Customers.Add(secondCustomer);

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
        finally
        {
            await using var cleanupContext = CreateContext();

            var customers = await cleanupContext.Customers.Where(customer => customer.Id == firstCustomerId || customer.Id == secondCustomerId).ToListAsync();

            cleanupContext.Customers.RemoveRange(customers);

            await cleanupContext.SaveChangesAsync();
        }
    }

}