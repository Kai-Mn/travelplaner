using Microsoft.EntityFrameworkCore;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Infrastructure.Persistence;

namespace TravelPlaner.Infrastructure.Repositories;

public class JourneyRepository(AppDbContext db) : IJourneyRepository
{
    public Task<Journey?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Journeys
            .Include(j => j.JourneyLocations)
                .ThenInclude(jl => jl.Location)
                    .ThenInclude(l => l.Images)
            .Include(j => j.JourneyLocations)
                .ThenInclude(jl => jl.Location)
                    .ThenInclude(l => l.Tags)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<IReadOnlyList<Journey>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        => await db.Journeys
            .Include(j => j.JourneyLocations)
            .Where(j => j.UserId == userId)
            .ToListAsync(ct);

    public async Task AddAsync(Journey journey, CancellationToken ct)
        => await db.Journeys.AddAsync(journey, ct);

    public Task DeleteAsync(Journey journey, CancellationToken ct)
    {
        db.Journeys.Remove(journey);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => db.SaveChangesAsync(ct);
}
