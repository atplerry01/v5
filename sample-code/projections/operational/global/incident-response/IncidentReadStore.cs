using Whycespace.Shared.Contracts.Infrastructure;

namespace Whycespace.Projections.Operational.Incident;

public sealed class IncidentReadStore
{
    private const string Projection = "incident";
    private const string TimelineProjection = "incident.timeline";
    private const string MetricsKey = "incident:metrics";

    private readonly IProjectionStore _store;

    public IncidentReadStore(IProjectionStore store)
    {
        _store = store;
    }

    public Task<IncidentReadModel?> GetAsync(string incidentId, CancellationToken ct = default)
        => _store.GetAsync<IncidentReadModel>(Projection, incidentId, ct);

    public Task SetAsync(string incidentId, IncidentReadModel model, CancellationToken ct = default)
        => _store.SetAsync(Projection, incidentId, model, ct);

    public Task<IReadOnlyList<IncidentReadModel>> GetAllAsync(CancellationToken ct = default)
        => _store.GetAllAsync<IncidentReadModel>(Projection, ct);

    public Task<IncidentTimelineReadModel?> GetTimelineAsync(string incidentId, CancellationToken ct = default)
        => _store.GetAsync<IncidentTimelineReadModel>(TimelineProjection, incidentId, ct);

    public Task SetTimelineAsync(string incidentId, IncidentTimelineReadModel model, CancellationToken ct = default)
        => _store.SetAsync(TimelineProjection, incidentId, model, ct);

    public Task<IncidentMetricsReadModel?> GetMetricsAsync(CancellationToken ct = default)
        => _store.GetAsync<IncidentMetricsReadModel>(Projection, MetricsKey, ct);

    public Task SetMetricsAsync(IncidentMetricsReadModel model, CancellationToken ct = default)
        => _store.SetAsync(Projection, MetricsKey, model, ct);
}
