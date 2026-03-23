using MediatR;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Domain.Exceptions;

namespace TravelPlaner.Application.Features.Locations;

public record DeleteLocationCommand(Guid LocationId, Guid UserId) : IRequest;

public class DeleteLocationCommandHandler(ILocationRepository locationRepo, IImageStore imageStore)
    : IRequestHandler<DeleteLocationCommand>
{
    public async Task Handle(DeleteLocationCommand request, CancellationToken ct)
    {
        var location = await locationRepo.GetByIdAsync(request.LocationId, ct)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        if (location.UserId != request.UserId)
            throw new UnauthorizedException();

        foreach (var image in location.Images)
            await imageStore.DeleteAsync(image.FilePath, ct);

        await locationRepo.DeleteAsync(location, ct);
        await locationRepo.SaveChangesAsync(ct);
    }
}
