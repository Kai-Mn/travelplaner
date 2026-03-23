namespace TravelPlaner.Domain.Entities;

public class Image
{
    public Guid Id { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public Guid LocationId { get; private set; }
    public Location Location { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private Image() { }

    public static Image Create(string filePath, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required.", nameof(filePath));

        return new Image
        {
            Id = Guid.NewGuid(),
            FilePath = filePath,
            LocationId = locationId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
