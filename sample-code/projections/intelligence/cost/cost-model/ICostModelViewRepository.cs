namespace Whycespace.Projections.Intelligence.Cost.CostModel;

public interface ICostModelViewRepository
{
    Task SaveAsync(CostModelReadModel model, CancellationToken ct = default);
    Task<CostModelReadModel?> GetAsync(string id, CancellationToken ct = default);
}
