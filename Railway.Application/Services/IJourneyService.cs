using Railway.Application.DTO;

namespace Railway.Application.Services;

public interface IJourneyService
{
    Task<IReadOnlyList<JourneyResponse>> SearchAsync(JourneySearchRequest request);

    Task<JourneyResponse?> GetByIdAsync(Guid journeyId);
}