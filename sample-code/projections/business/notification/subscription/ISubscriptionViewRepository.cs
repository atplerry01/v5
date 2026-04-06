namespace Whycespace.Projections.Business.Notification.Subscription;

public interface ISubscriptionViewRepository
{
    Task SaveAsync(SubscriptionReadModel model, CancellationToken ct = default);
    Task<SubscriptionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
