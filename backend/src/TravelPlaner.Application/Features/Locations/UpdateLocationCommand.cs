using MediatR;
using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Application.Mapping;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Domain.Exceptions;
using TravelPlaner.Domain.ValueObjects;

namespace TravelPlaner.Application.Features.Locations;

public record UpdateLocationCommand(Guid LocationId, string Name, CoordinatesDto Coordinates, string Description, Guid UserId)
    : IRequest<LocationDto>;

public class UpdateLocationCommandHandler(ILocationRepository repo)
    : IRequestHandler<UpdateLocationCommand, LocationDto>
{
    public async Task<LocationDto> Handle(UpdateLocationCommand request, CancellationToken ct)
    {
        var location = await repo.GetByIdAsync(request.LocationId, ct)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        if (location.UserId != request.UserId)
            throw new UnauthorizedException();

        var coords = Coordinates.Create(request.Coordinates.Latitude, request.Coordinates.Longitude);
        location.Update(request.Name, coords, request.Description);
        await repo.SaveChangesAsync(ct);
        return location.ToDto();
    }
}
