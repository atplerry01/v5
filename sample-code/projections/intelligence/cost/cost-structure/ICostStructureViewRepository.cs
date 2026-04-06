namespace Whycespace.Projections.Intelligence.Cost.CostStructure;

public interface ICostStructureViewRepository
{
    Task SaveAsync(CostStructureReadModel model, CancellationToken ct = default);
    Task<CostStructureReadModel?> GetAsync(string id, CancellationToken ct = default);
}
