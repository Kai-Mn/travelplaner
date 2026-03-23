using TravelPlaner.Domain.Entities;

namespace TravelPlaner.Application.Interfaces;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Location>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Location location, CancellationToken ct = default);
    Task DeleteAsync(Location location, CancellationToken ct = default);
    Task DeleteImageAsync(Image image, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
