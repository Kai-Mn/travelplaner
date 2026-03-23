namespace TravelPlaner.Application.DTOs;

public record TagDto(Guid Id, string Name);
public record AddTagRequest(string Name);
