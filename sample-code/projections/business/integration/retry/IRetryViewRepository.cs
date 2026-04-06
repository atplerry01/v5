namespace Whycespace.Projections.Business.Integration.Retry;

public interface IRetryViewRepository
{
    Task SaveAsync(RetryReadModel model, CancellationToken ct = default);
    Task<RetryReadModel?> GetAsync(string id, CancellationToken ct = default);
}
