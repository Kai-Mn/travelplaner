using MediatR;
using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Domain.Exceptions;

namespace TravelPlaner.Application.Features.Tags;

public record AddTagCommand(Guid LocationId, string Name, Guid UserId) : IRequest<TagDto>;

public class AddTagCommandHandler(ILocationRepository repo)
    : IRequestHandler<AddTagCommand, TagDto>
{
    public async Task<TagDto> Handle(AddTagCommand request, CancellationToken ct)
    {
        var location = await repo.GetByIdAsync(request.LocationId, ct)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        if (location.UserId != request.UserId)
            throw new UnauthorizedException();

        var tag = location.AddTag(request.Name);
        await repo.SaveChangesAsync(ct);
        return new TagDto(tag.Id, tag.Name);
    }
}

public record RemoveTagCommand(Guid LocationId, Guid TagId, Guid UserId) : IRequest;

public class RemoveTagCommandHandler(ILocationRepository repo)
    : IRequestHandler<RemoveTagCommand>
{
    public async Task Handle(RemoveTagCommand request, CancellationToken ct)
    {
        var location = await repo.GetByIdAsync(request.LocationId, ct)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        if (location.UserId != request.UserId)
            throw new UnauthorizedException();

        location.RemoveTag(request.TagId);
        await repo.SaveChangesAsync(ct);
    }
}
