namespace Whycespace.Projections.Business.Marketplace.Listing;

public interface IListingViewRepository
{
    Task SaveAsync(ListingReadModel model, CancellationToken ct = default);
    Task<ListingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
