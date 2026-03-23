namespace TravelPlaner.Domain.Entities;

public class JourneyLocation
{
    public Guid JourneyId { get; private set; }
    public Journey Journey { get; private set; } = null!;
    public Guid LocationId { get; private set; }
    public Location Location { get; private set; } = null!;
    public DateTime AddedAt { get; private set; }

    private JourneyLocation() { }

    public static JourneyLocation Create(Guid journeyId, Guid locationId)
    {
        return new JourneyLocation
        {
            JourneyId = journeyId,
            LocationId = locationId,
            AddedAt = DateTime.UtcNow
        };
    }
}
