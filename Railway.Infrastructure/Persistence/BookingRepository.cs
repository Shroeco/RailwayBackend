using Microsoft.EntityFrameworkCore;
using Railway.Application.Exceptions;
using Railway.Application.Repositories;
using Railway.Domain.Entities;
using Railway.Infrastructure.Data;

namespace Railway.Infrastructure.Data;

 public sealed class BookingRepository : IBookingRepository
{
    private readonly RailwayDbContext _context;

    public BookingRepository(RailwayDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid bookingId)
    {
        return await _context.Bookings.SingleOrDefaultAsync(booking => booking.Id == bookingId);
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
    }

    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }

    public async Task CreateBookingTransactionAsync(Booking booking, Journey journey)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Journeys.Update(journey);
            await _context.Bookings.AddAsync(booking);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw new BookingConflictException();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}