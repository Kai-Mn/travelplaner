namespace TravelPlaner.Domain.Entities;

public class Tag
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid LocationId { get; private set; }
    public Location Location { get; private set; } = null!;

    private Tag() { }

    public static Tag Create(string name, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name is required.", nameof(name));

        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            LocationId = locationId
        };
    }
}
