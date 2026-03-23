using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Domain.Entities;

namespace TravelPlaner.Application.Mapping;

public static class MappingExtensions
{
    public static CoordinatesDto ToDto(this Domain.ValueObjects.Coordinates coords)
        => new(coords.Latitude, coords.Longitude);

    public static TagDto ToDto(this Tag tag)
        => new(tag.Id, tag.Name);

    public static ImageDto ToDto(this Image image, IImageStore imageStore)
        => new(image.Id, imageStore.GetPublicUrl(image.FilePath));

    public static LocationDto ToDto(this Location location, IImageStore? imageStore = null)
        => new(
            location.Id,
            location.Name,
            location.Coordinates.ToDto(),
            location.Description,
            location.CreatedAt,
            location.Tags.Select(t => t.ToDto()).ToList().AsReadOnly(),
            imageStore is not null
                ? location.Images.Select(i => i.ToDto(imageStore)).ToList().AsReadOnly()
                : location.Images.Select(i => new ImageDto(i.Id, i.FilePath)).ToList().AsReadOnly()
        );

    public static JourneySummaryDto ToSummaryDto(this Journey journey)
        => new(journey.Id, journey.Name, journey.Description, journey.CreatedAt,
               journey.JourneyLocations.Count);

    public static JourneyDto ToDto(this Journey journey, IImageStore? imageStore = null)
        => new(
            journey.Id,
            journey.Name,
            journey.Description,
            journey.CreatedAt,
            journey.JourneyLocations
                .Select(jl => jl.Location.ToDto(imageStore))
                .ToList().AsReadOnly()
        );
}
