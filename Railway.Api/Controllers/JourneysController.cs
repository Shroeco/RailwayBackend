using Microsoft.AspNetCore.Mvc;
using Railway.Application.DTO;
using Railway.Application.Services;

namespace Railway.Api.Controllers;

[ApiController]
[Route("api/journeys")]
public sealed class JourneysController : ControllerBase
{
    private readonly IJourneyService _journeyService;

    public JourneysController(IJourneyService journeyService)
    {
        _journeyService = journeyService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<JourneyResponse>>> Search([FromQuery] JourneySearchRequest request)
    {
        var journeys = await _journeyService.SearchAsync(request);

        return Ok(journeys);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JourneyResponse>> GetById(Guid id)
    {
        var journey = await _journeyService.GetByIdAsync(id);

        if (journey is null)
            return NotFound();
        
        return Ok(journey);
    }
}