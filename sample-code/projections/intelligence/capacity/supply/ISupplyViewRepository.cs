namespace Whycespace.Projections.Intelligence.Capacity.Supply;

public interface ISupplyViewRepository
{
    Task SaveAsync(SupplyReadModel model, CancellationToken ct = default);
    Task<SupplyReadModel?> GetAsync(string id, CancellationToken ct = default);
}
