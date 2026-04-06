namespace Whycespace.Projections.Intelligence.Index.CostIndex;

public interface ICostIndexViewRepository
{
    Task SaveAsync(CostIndexReadModel model, CancellationToken ct = default);
    Task<CostIndexReadModel?> GetAsync(string id, CancellationToken ct = default);
}
