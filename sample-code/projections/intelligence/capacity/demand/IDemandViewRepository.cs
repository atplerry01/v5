namespace Whycespace.Projections.Intelligence.Capacity.Demand;

public interface IDemandViewRepository
{
    Task SaveAsync(DemandReadModel model, CancellationToken ct = default);
    Task<DemandReadModel?> GetAsync(string id, CancellationToken ct = default);
}
