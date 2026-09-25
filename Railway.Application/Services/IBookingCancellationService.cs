namespace Railway.Application.Services;

public interface IBookingCancellationService
{
    Task CancelAsync(Guid bookingId);
}