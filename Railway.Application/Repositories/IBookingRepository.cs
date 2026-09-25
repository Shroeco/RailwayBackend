using Railway.Domain.Entities;

namespace Railway.Application.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid bookingId);

    Task AddAsync(Booking booking);

    Task UpdateAsync(Booking booking);

    Task CreateBookingTransactionAsync(Booking booking, Journey journey);
}