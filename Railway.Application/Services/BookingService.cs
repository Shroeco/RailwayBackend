using Railway.Application.DTO;
using Railway.Application.Repositories;
using Railway.Domain.Entities;

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
        var customer = await _customerRepository
            .GetByIdAsync(request.CustomerId);

        if (customer is null)
            throw new InvalidOperationException("Customer was not found.");

        var journey = await _journeyRepository
            .GetByIdAsync(request.JourneyId);

        if (journey is null)
            throw new InvalidOperationException("Journey was not found.");

        var fare = journey.Fares
            .SingleOrDefault(fare => fare.Id == request.FareId);

        if (fare is null)
            throw new InvalidOperationException(
                "Fare does not belong to the specified journey.");

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
}