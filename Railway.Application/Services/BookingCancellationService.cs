using Railway.Application.Repositories;

namespace Railway.Application.Services;

public sealed class BookingCancellationService : IBookingCancellationService
{
    private readonly IBookingRepository _bookingRepository;

    public BookingCancellationService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task CancelAsync(Guid bookingId)
    {
        var booking = await _bookingRepository
            .GetByIdAsync(bookingId);

        if (booking is null)
            throw new InvalidOperationException("Booking was not found.");

        booking.Cancel();

        await _bookingRepository.UpdateAsync(booking);
    }
}