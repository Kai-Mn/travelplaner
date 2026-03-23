namespace TravelPlaner.Domain.Entities;

public class Journey
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    private readonly List<JourneyLocation> _journeyLocations = [];
    public IReadOnlyCollection<JourneyLocation> JourneyLocations => _journeyLocations.AsReadOnly();

    private Journey() { }

    public static Journey Create(string name, string description, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Journey name is required.", nameof(name));

        return new Journey
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description ?? string.Empty,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Journey name is required.", nameof(name));

        Name = name.Trim();
        Description = description ?? string.Empty;
    }

    public JourneyLocation AddLocation(Guid locationId)
    {
        if (_journeyLocations.Any(jl => jl.LocationId == locationId))
            throw new InvalidOperationException("Location is already part of this journey.");

        var jl = JourneyLocation.Create(Id, locationId);
        _journeyLocations.Add(jl);
        return jl;
    }

    public void RemoveLocation(Guid locationId)
    {
        var jl = _journeyLocations.FirstOrDefault(jl => jl.LocationId == locationId)
            ?? throw new InvalidOperationException($"Location {locationId} is not part of this journey.");
        _journeyLocations.Remove(jl);
    }
}
