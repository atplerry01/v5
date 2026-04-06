namespace Whycespace.Projections.Operational.Incident;

public sealed class IncidentQuery
{
    private readonly IncidentReadStore _store;

    public IncidentQuery(IncidentReadStore store)
    {
        _store = store;
    }

    public Task<IncidentReadModel?> GetByIdAsync(string incidentId, CancellationToken ct = default)
        => _store.GetAsync(incidentId, ct);

    public Task<IReadOnlyList<IncidentReadModel>> ListAsync(CancellationToken ct = default)
        => _store.GetAllAsync(ct);

    public Task<IncidentTimelineReadModel?> GetTimelineAsync(string incidentId, CancellationToken ct = default)
        => _store.GetTimelineAsync(incidentId, ct);

    public Task<IncidentMetricsReadModel?> GetMetricsAsync(CancellationToken ct = default)
        => _store.GetMetricsAsync(ct);
}
