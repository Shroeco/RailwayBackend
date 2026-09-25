using Microsoft.AspNetCore.Mvc;
using Railway.Application.DTO;
using Railway.Application.Services;

namespace Railway.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IBookingCancellationService _bookingCancellationService;

    public BookingsController(IBookingService bookingService, IBookingCancellationService bookingCancellationService)
    {
        _bookingService = bookingService;
        _bookingCancellationService = bookingCancellationService;
    }

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create([FromBody] CreateBookingRequest request)
    {
        var booking = await _bookingService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> GetById(Guid id)
    {
        var booking = await _bookingService.GetByIdAsync(id);

        if (booking is null)
            return NotFound();

        return Ok(booking);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _bookingCancellationService.CancelAsync(id);

        return NoContent();
    }
}