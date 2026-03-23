using MediatR;
using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Application.Mapping;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Domain.Exceptions;

namespace TravelPlaner.Application.Features.Journeys;

public record GetJourneyQuery(Guid JourneyId, Guid UserId) : IRequest<JourneyDto>;

public class GetJourneyQueryHandler(IJourneyRepository repo)
    : IRequestHandler<GetJourneyQuery, JourneyDto>
{
    public async Task<JourneyDto> Handle(GetJourneyQuery request, CancellationToken ct)
    {
        var journey = await repo.GetByIdAsync(request.JourneyId, ct)
            ?? throw new NotFoundException(nameof(Journey), request.JourneyId);

        if (journey.UserId != request.UserId)
            throw new UnauthorizedException();

        return journey.ToDto();
    }
}

public record GetJourneysQuery(Guid UserId) : IRequest<IReadOnlyList<JourneySummaryDto>>;

public class GetJourneysQueryHandler(IJourneyRepository repo)
    : IRequestHandler<GetJourneysQuery, IReadOnlyList<JourneySummaryDto>>
{
    public async Task<IReadOnlyList<JourneySummaryDto>> Handle(GetJourneysQuery request, CancellationToken ct)
    {
        var journeys = await repo.GetByUserIdAsync(request.UserId, ct);
        return journeys.Select(j => j.ToSummaryDto()).ToList().AsReadOnly();
    }
}
