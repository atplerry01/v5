namespace Whycespace.Projections.Business.Subscription.Usage;

public interface IUsageViewRepository
{
    Task SaveAsync(UsageReadModel model, CancellationToken ct = default);
    Task<UsageReadModel?> GetAsync(string id, CancellationToken ct = default);
}
