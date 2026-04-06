namespace Whycespace.Projections.Business.Inventory.Valuation;

public interface IValuationViewRepository
{
    Task SaveAsync(ValuationReadModel model, CancellationToken ct = default);
    Task<ValuationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
