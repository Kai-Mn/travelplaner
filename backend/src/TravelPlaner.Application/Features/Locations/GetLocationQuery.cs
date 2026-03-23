using MediatR;
using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Application.Mapping;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Domain.Exceptions;

namespace TravelPlaner.Application.Features.Locations;

public record GetLocationQuery(Guid LocationId, Guid UserId) : IRequest<LocationDto>;

public class GetLocationQueryHandler(ILocationRepository repo)
    : IRequestHandler<GetLocationQuery, LocationDto>
{
    public async Task<LocationDto> Handle(GetLocationQuery request, CancellationToken ct)
    {
        var location = await repo.GetByIdAsync(request.LocationId, ct)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        if (location.UserId != request.UserId)
            throw new UnauthorizedException();

        return location.ToDto();
    }
}

public record GetLocationsQuery(Guid UserId) : IRequest<IReadOnlyList<LocationDto>>;

public class GetLocationsQueryHandler(ILocationRepository repo)
    : IRequestHandler<GetLocationsQuery, IReadOnlyList<LocationDto>>
{
    public async Task<IReadOnlyList<LocationDto>> Handle(GetLocationsQuery request, CancellationToken ct)
    {
        var locations = await repo.GetByUserIdAsync(request.UserId, ct);
        return locations.Select(l => l.ToDto()).ToList().AsReadOnly();
    }
}
