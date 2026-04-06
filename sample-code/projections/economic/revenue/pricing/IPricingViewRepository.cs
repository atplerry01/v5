namespace Whycespace.Projections.Economic.Revenue.Pricing;

public interface IPricingViewRepository
{
    Task SaveAsync(PricingReadModel model, CancellationToken ct = default);
    Task<PricingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
