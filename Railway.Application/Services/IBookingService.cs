using Railway.Application.DTO;

namespace Railway.Application.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(CreateBookingRequest request);
}