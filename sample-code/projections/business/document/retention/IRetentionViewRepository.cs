namespace Whycespace.Projections.Business.Document.Retention;

public interface IRetentionViewRepository
{
    Task SaveAsync(RetentionReadModel model, CancellationToken ct = default);
    Task<RetentionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
