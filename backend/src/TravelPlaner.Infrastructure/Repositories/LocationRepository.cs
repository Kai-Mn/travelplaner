using Microsoft.EntityFrameworkCore;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Infrastructure.Persistence;

namespace TravelPlaner.Infrastructure.Repositories;

public class LocationRepository(AppDbContext db) : ILocationRepository
{
    public Task<Location?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Locations
            .Include(l => l.Images)
            .Include(l => l.Tags)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<Location>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        => await db.Locations
            .Include(l => l.Images)
            .Include(l => l.Tags)
            .Where(l => l.UserId == userId)
            .ToListAsync(ct);

    public async Task AddAsync(Location location, CancellationToken ct)
        => await db.Locations.AddAsync(location, ct);

    public Task DeleteAsync(Location location, CancellationToken ct)
    {
        db.Locations.Remove(location);
        return Task.CompletedTask;
    }

    public Task DeleteImageAsync(Image image, CancellationToken ct)
    {
        db.Images.Remove(image);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => db.SaveChangesAsync(ct);
}
