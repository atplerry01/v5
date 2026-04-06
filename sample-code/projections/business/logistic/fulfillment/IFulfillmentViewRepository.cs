namespace Whycespace.Projections.Business.Logistic.Fulfillment;

public interface IFulfillmentViewRepository
{
    Task SaveAsync(FulfillmentReadModel model, CancellationToken ct = default);
    Task<FulfillmentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
