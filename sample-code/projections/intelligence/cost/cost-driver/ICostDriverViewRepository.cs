namespace Whycespace.Projections.Intelligence.Cost.CostDriver;

public interface ICostDriverViewRepository
{
    Task SaveAsync(CostDriverReadModel model, CancellationToken ct = default);
    Task<CostDriverReadModel?> GetAsync(string id, CancellationToken ct = default);
}
