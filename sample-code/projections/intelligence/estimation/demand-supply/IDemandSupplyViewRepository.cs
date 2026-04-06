namespace Whycespace.Projections.Intelligence.Estimation.DemandSupply;

public interface IDemandSupplyViewRepository
{
    Task SaveAsync(DemandSupplyReadModel model, CancellationToken ct = default);
    Task<DemandSupplyReadModel?> GetAsync(string id, CancellationToken ct = default);
}
