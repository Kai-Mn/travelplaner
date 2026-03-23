using TravelPlaner.Domain.ValueObjects;

namespace TravelPlaner.Domain.Entities;

public class Location
{
    public Guid Id { get; private set; }
    public Coordinates Coordinates { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    private readonly List<Image> _images = [];
    private readonly List<Tag> _tags = [];
    private readonly List<JourneyLocation> _journeyLocations = [];

    public IReadOnlyCollection<Image> Images => _images.AsReadOnly();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    public IReadOnlyCollection<JourneyLocation> JourneyLocations => _journeyLocations.AsReadOnly();

    private Location() { }

    public static Location Create(string name, Coordinates coordinates, string description, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name is required.", nameof(name));
        ArgumentNullException.ThrowIfNull(coordinates);

        return new Location
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Coordinates = coordinates,
            Description = description ?? string.Empty,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, Coordinates coordinates, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name is required.", nameof(name));
        ArgumentNullException.ThrowIfNull(coordinates);

        Name = name.Trim();
        Coordinates = coordinates;
        Description = description ?? string.Empty;
    }

    public Tag AddTag(string name)
    {
        var tag = Tag.Create(name, Id);
        _tags.Add(tag);
        return tag;
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = _tags.FirstOrDefault(t => t.Id == tagId)
            ?? throw new InvalidOperationException($"Tag {tagId} not found on this location.");
        _tags.Remove(tag);
    }

    public Image AddImage(string filePath)
    {
        var image = Image.Create(filePath, Id);
        _images.Add(image);
        return image;
    }
}
