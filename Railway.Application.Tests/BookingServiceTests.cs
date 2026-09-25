using Railway.Application.DTO;
using Railway.Application.Exceptions;
using Railway.Application.Repositories;
using Railway.Application.Services;
using Railway.Domain.Entities;

namespace Railway.Application.Tests;

public class BookingServiceTests
{
    [Fact]
    public async Task CreateAsync_should_throw_when_customer_does_not_exist()
    {
        var customerRepository = new FakeCustomerRepository();
        var journeyRepository = new FakeJourneyRepository();
        var bookingRepository = new FakeBookingRepository();

        var service = new BookingService( customerRepository, journeyRepository, bookingRepository);

        var request = new CreateBookingRequest
        {
            CustomerId = Guid.NewGuid(),
            JourneyId = Guid.NewGuid(),
            FareId = Guid.NewGuid()
        };

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => service.CreateAsync(request));

        Assert.Equal("Customer was not found.", exception.Message);
    }
    [Fact]
    public async Task CreateAsync_should_throw_when_journey_does_not_exist()
    {
        var customerId = Guid.NewGuid();

        var customerRepository = new FakeCustomerRepository(new Customer(customerId, "Test Customer", "test@example.com"));

        var journeyRepository = new FakeJourneyRepository();
        var bookingRepository = new FakeBookingRepository();

        var service = new BookingService( customerRepository, journeyRepository, bookingRepository);

        var request = new CreateBookingRequest
        {
            CustomerId = customerId,
            JourneyId = Guid.NewGuid(),
            FareId = Guid.NewGuid()
        };

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => service.CreateAsync(request));

        Assert.Equal("Journey was not found.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_should_throw_when_fare_does_not_belong_to_journey()
    {
        var customerId = Guid.NewGuid();
        var journeyId = Guid.NewGuid();
        var fareId = Guid.NewGuid();

        var customer = new Customer(customerId, "Test Customer", "test@example.com");

        var journey = new Journey(journeyId, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 10);

        var customerRepository = new FakeCustomerRepository(customer);
        var journeyRepository = new FakeJourneyRepository(journey);
        var bookingRepository = new FakeBookingRepository();

        var service = new BookingService( customerRepository, journeyRepository, bookingRepository);

        var request = new CreateBookingRequest
        {
            CustomerId = customerId,
            JourneyId = journeyId,
            FareId = fareId
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));

        Assert.Equal("Fare does not belong to the specified journey.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_should_throw_when_no_seats_are_available()
    {
        var customerId = Guid.NewGuid();
        var journeyId = Guid.NewGuid();
        var fareId = Guid.NewGuid();

        var customer = new Customer(customerId, "Test Customer", "test@example.com");

        var journey = new Journey( journeyId, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 1);

        var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

        journey.Fares.Add(fare);

        journey.ReserveSeat();

        var customerRepository = new FakeCustomerRepository(customer);
        var journeyRepository = new FakeJourneyRepository(journey);
        var bookingRepository = new FakeBookingRepository();

        var service = new BookingService(customerRepository, journeyRepository, bookingRepository);

        var request = new CreateBookingRequest
        {
            CustomerId = customerId,
            JourneyId = journeyId,
            FareId = fareId
        };

        var exception = await Assert.ThrowsAsync<BookingUnavailableException>(() => service.CreateAsync(request));

        Assert.Equal("The journey has no available seats.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_should_propagate_booking_conflict()
    {
        var customerId = Guid.NewGuid();
        var journeyId = Guid.NewGuid();
        var fareId = Guid.NewGuid();

        var customer = new Customer(customerId, "Test Customer", "test@example.com");

        var journey = new Journey( journeyId, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 1);

        var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

        journey.Fares.Add(fare);

        var customerRepository = new FakeCustomerRepository(customer);
        var journeyRepository = new FakeJourneyRepository(journey);
        var bookingRepository = new FakeBookingRepository(throwBookingConflict: true);

        var service = new BookingService(customerRepository, journeyRepository, bookingRepository);

        var request = new CreateBookingRequest
        {
            CustomerId = customerId,
            JourneyId = journeyId,
            FareId = fareId
        };

        await Assert.ThrowsAsync<BookingConflictException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_should_create_booking_when_request_is_valid()
    {
        var customerId = Guid.NewGuid();
        var journeyId = Guid.NewGuid();
        var fareId = Guid.NewGuid();

        var customer = new Customer(customerId, "Test Customer", "test@example.com");

        var journey = new Journey(journeyId, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 10);

        var fare = new Fare(fareId, journeyId, "Standard", 25.00m);

        journey.Fares.Add(fare);

        var customerRepository = new FakeCustomerRepository(customer);
        var journeyRepository = new FakeJourneyRepository(journey);
        var bookingRepository = new FakeBookingRepository();

        var service = new BookingService(customerRepository, journeyRepository, bookingRepository);

        var request = new CreateBookingRequest
        {
            CustomerId = customerId,
            JourneyId = journeyId,
            FareId = fareId
        };

        var response = await service.CreateAsync(request);

        Assert.NotNull(bookingRepository.CreatedBooking);
        Assert.Equal(customerId, bookingRepository.CreatedBooking.CustomerId);
        Assert.Equal(journeyId, bookingRepository.CreatedBooking.JourneyId);
        Assert.Equal(fareId, bookingRepository.CreatedBooking.FareId);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(customerId, response.CustomerId);
        Assert.Equal(journeyId, response.JourneyId);
        Assert.Equal(fareId, response.FareId);
        Assert.Equal("Confirmed", response.Status);

        Assert.Equal(9, journey.AvailableSeats);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        private readonly Customer? _customer;

        public FakeCustomerRepository(Customer? customer = null)
        {
            _customer = customer;
        }

        public Task<Customer?> GetByIdAsync(Guid customerId)
        {
            return Task.FromResult(_customer);
        }

        public Task AddAsync(Customer customer)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeJourneyRepository : IJourneyRepository
    {
        private readonly Journey? _journey;

        public FakeJourneyRepository(Journey? journey = null)
        {
            _journey = journey;
        }

        public Task<Journey?> GetByIdAsync(Guid journeyId)
        {
            return Task.FromResult(_journey);
        }

        public Task<IReadOnlyList<Journey>> SearchAsync(Guid originStationId, Guid destinationStationId, DateTimeOffset departureFrom, DateTimeOffset departureTo)
        {
            return Task.FromResult<IReadOnlyList<Journey>>(Array.Empty<Journey>());
        }

        public Task UpdateAsync(Journey journey)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBookingRepository : IBookingRepository
    {
        private readonly bool _throwBookingConflict;

        public Booking? CreatedBooking { get; private set; }

        public FakeBookingRepository(bool throwBookingConflict = false)
        {
            _throwBookingConflict = throwBookingConflict;
        }
        public Task<Booking?> GetByIdAsync(Guid bookingId)
        {
            return Task.FromResult<Booking?>(null);
        }

        public Task AddAsync(Booking booking)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Booking booking)
        {
            return Task.CompletedTask;
        }

        public Task CreateBookingTransactionAsync(Booking booking, Journey journey)
        {
            if (_throwBookingConflict)
                throw new BookingConflictException();
            
            CreatedBooking = booking;

            return Task.CompletedTask;
        }
    }
}