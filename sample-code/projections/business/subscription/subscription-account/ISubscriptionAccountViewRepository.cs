namespace Whycespace.Projections.Business.Subscription.SubscriptionAccount;

public interface ISubscriptionAccountViewRepository
{
    Task SaveAsync(SubscriptionAccountReadModel model, CancellationToken ct = default);
    Task<SubscriptionAccountReadModel?> GetAsync(string id, CancellationToken ct = default);
}
