using TravelPlaner.Domain.Entities;

namespace TravelPlaner.Application.Interfaces;

public interface IJourneyRepository
{
    Task<Journey?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Journey>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Journey journey, CancellationToken ct = default);
    Task DeleteAsync(Journey journey, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
