namespace Whycespace.Projections.Business.Subscription.Cancellation;

public interface ICancellationViewRepository
{
    Task SaveAsync(CancellationReadModel model, CancellationToken ct = default);
    Task<CancellationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
