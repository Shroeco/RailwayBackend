using Railway.Application.DTO;
using Railway.Application.Repositories;
using Railway.Domain.Entities;
using Railway.Application.Exceptions;

namespace Railway.Application.Services;

public sealed class BookingService : IBookingService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IJourneyRepository _journeyRepository;
    private readonly IBookingRepository _bookingRepository;

    public BookingService(ICustomerRepository customerRepository, IJourneyRepository journeyRepository, IBookingRepository bookingRepository)
    {
        _customerRepository = customerRepository;
        _journeyRepository = journeyRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request)
    {
        if (request.CustomerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required.");

        if (request.JourneyId == Guid.Empty)
            throw new ArgumentException("Journey ID is required.");

        if (request.FareId == Guid.Empty)
            throw new ArgumentException("Fare ID is required.");

        var customer = await _customerRepository
            .GetByIdAsync(request.CustomerId);

        if (customer is null)
            throw new ResourceNotFoundException("Customer");

        var journey = await _journeyRepository
            .GetByIdAsync(request.JourneyId);

        if (journey is null)
            throw new ResourceNotFoundException("Journey");

        var fare = journey.Fares
            .SingleOrDefault(fare => fare.Id == request.FareId);

        if (fare is null)
            throw new InvalidOperationException(
                "Fare does not belong to the specified journey.");

        if (journey.AvailableSeats <= 0)
            throw new BookingUnavailableException();

        journey.ReserveSeat();

        var booking = new Booking(
            Guid.NewGuid(),
            customer.Id,
            journey.Id,
            fare.Id,
            DateTimeOffset.UtcNow);

        await _bookingRepository.CreateBookingTransactionAsync(
            booking,
            journey);

        return new BookingResponse
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            JourneyId = booking.JourneyId,
            FareId = booking.FareId,
            BookedAt = booking.BookedAt,
            Status = booking.Status.ToString()
        };
    }

    public async Task<BookingResponse?> GetByIdAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);

        if (booking is null)
            return null;

        return new BookingResponse
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            JourneyId = booking.JourneyId,
            FareId = booking.FareId,
            BookedAt = booking.BookedAt,
            Status = booking.Status.ToString()
        };
    }
}