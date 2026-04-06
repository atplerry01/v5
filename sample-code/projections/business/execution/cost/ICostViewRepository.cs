namespace Whycespace.Projections.Business.Execution.Cost;

public interface ICostViewRepository
{
    Task SaveAsync(CostReadModel model, CancellationToken ct = default);
    Task<CostReadModel?> GetAsync(string id, CancellationToken ct = default);
}
