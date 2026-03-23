namespace TravelPlaner.Application.DTOs;

public record JourneyDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    IReadOnlyList<LocationDto> Locations
);

public record JourneySummaryDto(Guid Id, string Name, string Description, DateTime CreatedAt, int LocationCount);
public record CreateJourneyRequest(string Name, string Description);
public record UpdateJourneyRequest(string Name, string Description);
