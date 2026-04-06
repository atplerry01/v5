namespace Whycespace.Projections.Business.Marketplace.Bid;

public interface IBidViewRepository
{
    Task SaveAsync(BidReadModel model, CancellationToken ct = default);
    Task<BidReadModel?> GetAsync(string id, CancellationToken ct = default);
}
