using MediatR;
using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Application.Mapping;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Domain.ValueObjects;

namespace TravelPlaner.Application.Features.Locations;

public record CreateLocationCommand(string Name, CoordinatesDto Coordinates, string Description, Guid UserId)
    : IRequest<LocationDto>;

public class CreateLocationCommandHandler(ILocationRepository repo)
    : IRequestHandler<CreateLocationCommand, LocationDto>
{
    public async Task<LocationDto> Handle(CreateLocationCommand request, CancellationToken ct)
    {
        var coords = Coordinates.Create(request.Coordinates.Latitude, request.Coordinates.Longitude);
        var location = Location.Create(request.Name, coords, request.Description, request.UserId);
        await repo.AddAsync(location, ct);
        await repo.SaveChangesAsync(ct);
        return location.ToDto();
    }
}
