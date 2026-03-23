using MediatR;
using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Application.Mapping;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Domain.Exceptions;

namespace TravelPlaner.Application.Features.Journeys;

public record CreateJourneyCommand(string Name, string Description, Guid UserId) : IRequest<JourneySummaryDto>;

public class CreateJourneyCommandHandler(IJourneyRepository repo)
    : IRequestHandler<CreateJourneyCommand, JourneySummaryDto>
{
    public async Task<JourneySummaryDto> Handle(CreateJourneyCommand request, CancellationToken ct)
    {
        var journey = Journey.Create(request.Name, request.Description, request.UserId);
        await repo.AddAsync(journey, ct);
        await repo.SaveChangesAsync(ct);
        return journey.ToSummaryDto();
    }
}

public record UpdateJourneyCommand(Guid JourneyId, string Name, string Description, Guid UserId) : IRequest<JourneySummaryDto>;

public class UpdateJourneyCommandHandler(IJourneyRepository repo)
    : IRequestHandler<UpdateJourneyCommand, JourneySummaryDto>
{
    public async Task<JourneySummaryDto> Handle(UpdateJourneyCommand request, CancellationToken ct)
    {
        var journey = await repo.GetByIdAsync(request.JourneyId, ct)
            ?? throw new NotFoundException(nameof(Journey), request.JourneyId);

        if (journey.UserId != request.UserId)
            throw new UnauthorizedException();

        journey.Update(request.Name, request.Description);
        await repo.SaveChangesAsync(ct);
        return journey.ToSummaryDto();
    }
}

public record DeleteJourneyCommand(Guid JourneyId, Guid UserId) : IRequest;

public class DeleteJourneyCommandHandler(IJourneyRepository repo)
    : IRequestHandler<DeleteJourneyCommand>
{
    public async Task Handle(DeleteJourneyCommand request, CancellationToken ct)
    {
        var journey = await repo.GetByIdAsync(request.JourneyId, ct)
            ?? throw new NotFoundException(nameof(Journey), request.JourneyId);

        if (journey.UserId != request.UserId)
            throw new UnauthorizedException();

        await repo.DeleteAsync(journey, ct);
        await repo.SaveChangesAsync(ct);
    }
}

public record AddLocationToJourneyCommand(Guid JourneyId, Guid LocationId, Guid UserId) : IRequest;

public class AddLocationToJourneyCommandHandler(IJourneyRepository journeyRepo, ILocationRepository locationRepo)
    : IRequestHandler<AddLocationToJourneyCommand>
{
    public async Task Handle(AddLocationToJourneyCommand request, CancellationToken ct)
    {
        var journey = await journeyRepo.GetByIdAsync(request.JourneyId, ct)
            ?? throw new NotFoundException(nameof(Journey), request.JourneyId);

        if (journey.UserId != request.UserId)
            throw new UnauthorizedException();

        _ = await locationRepo.GetByIdAsync(request.LocationId, ct)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        journey.AddLocation(request.LocationId);
        await journeyRepo.SaveChangesAsync(ct);
    }
}

public record RemoveLocationFromJourneyCommand(Guid JourneyId, Guid LocationId, Guid UserId) : IRequest;

public class RemoveLocationFromJourneyCommandHandler(IJourneyRepository repo)
    : IRequestHandler<RemoveLocationFromJourneyCommand>
{
    public async Task Handle(RemoveLocationFromJourneyCommand request, CancellationToken ct)
    {
        var journey = await repo.GetByIdAsync(request.JourneyId, ct)
            ?? throw new NotFoundException(nameof(Journey), request.JourneyId);

        if (journey.UserId != request.UserId)
            throw new UnauthorizedException();

        journey.RemoveLocation(request.LocationId);
        await repo.SaveChangesAsync(ct);
    }
}
