namespace Whycespace.Projections.Intelligence.Observability.Log;

public interface ILogViewRepository
{
    Task SaveAsync(LogReadModel model, CancellationToken ct = default);
    Task<LogReadModel?> GetAsync(string id, CancellationToken ct = default);
}
