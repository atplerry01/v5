namespace Whycespace.Projections.Business.Marketplace.SettlementMarket;

public interface ISettlementMarketViewRepository
{
    Task SaveAsync(SettlementMarketReadModel model, CancellationToken ct = default);
    Task<SettlementMarketReadModel?> GetAsync(string id, CancellationToken ct = default);
}
