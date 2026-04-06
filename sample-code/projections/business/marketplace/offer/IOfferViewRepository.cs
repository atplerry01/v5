namespace Whycespace.Projections.Business.Marketplace.Offer;

public interface IOfferViewRepository
{
    Task SaveAsync(OfferReadModel model, CancellationToken ct = default);
    Task<OfferReadModel?> GetAsync(string id, CancellationToken ct = default);
}
