using Railway.Application.Repositories;
using Railway.Application.Services;
using Railway.Application.Exceptions;
using Railway.Domain.Entities;

namespace Railway.Application.Tests;

public class BookingCancellationServiceTests
{
    [Fact]
    public async Task CancelAsync_should_throw_when_booking_does_not_exist()
    {
        var bookingRepository = new FakeBookingRepository();

        var service = new BookingCancellationService(bookingRepository);

        var bookingId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => service.CancelAsync(bookingId));

        Assert.Equal("Booking was not found.", exception.Message);
    }

    [Fact]
    public async Task CancelAsync_should_cancel_existing_booking()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        var bookingRepository = new FakeBookingRepository(booking);

        var service = new BookingCancellationService(bookingRepository);

        await service.CancelAsync(booking.Id);

        Assert.Equal(Railway.Domain.Enums.BookingStatus.Cancelled, booking.Status);

        Assert.Same(booking, bookingRepository.UpdatedBooking);
    }

    [Fact]
    public async Task CancelAsync_should_throw_when_booking_is_already_cancelled()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        booking.Cancel();

        var bookingRepository = new FakeBookingRepository(booking);
        var service = new BookingCancellationService(bookingRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelAsync(booking.Id));

        Assert.Equal("Booking is already cancelled", exception.Message);

        Assert.Null(bookingRepository.UpdatedBooking);
    }

    private sealed class FakeBookingRepository : IBookingRepository
    {
        private readonly Booking? _booking;

        public Booking? UpdatedBooking { get; private set; }

        public FakeBookingRepository(Booking? booking = null)
        {
            _booking = booking;
        }

        public Task<Booking?> GetByIdAsync(Guid bookingId)
        {
            return Task.FromResult(_booking);
        }

        public Task AddAsync(Booking booking)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Booking booking)
        {
            UpdatedBooking = booking;
            return Task.CompletedTask;
        }

        public Task CreateBookingTransactionAsync(Booking booking, Journey journey)
        {
            return Task.CompletedTask;
        }
    }
}