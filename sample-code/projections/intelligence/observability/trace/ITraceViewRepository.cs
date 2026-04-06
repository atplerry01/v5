namespace Whycespace.Projections.Intelligence.Observability.Trace;

public interface ITraceViewRepository
{
    Task SaveAsync(TraceReadModel model, CancellationToken ct = default);
    Task<TraceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
