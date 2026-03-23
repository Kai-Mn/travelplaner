namespace TravelPlaner.Application.DTOs;

public record LocationDto(
    Guid Id,
    string Name,
    CoordinatesDto Coordinates,
    string Description,
    DateTime CreatedAt,
    IReadOnlyList<TagDto> Tags,
    IReadOnlyList<ImageDto> Images
);

public record CreateLocationRequest(string Name, CoordinatesDto Coordinates, string Description);
public record UpdateLocationRequest(string Name, CoordinatesDto Coordinates, string Description);
