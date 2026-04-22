using Whycespace.Projections.Control.Observability.SystemMetric.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Observability.SystemMetric;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Observability.SystemMetric;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Observability.SystemMetric;

public sealed class SystemMetricProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SystemMetricDefinedEventSchema>,
    IProjectionHandler<SystemMetricDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<SystemMetricReadModel> _store;

    public SystemMetricProjectionHandler(PostgresProjectionStore<SystemMetricReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SystemMetricDefinedEventSchema e     => Project(e.AggregateId, s => SystemMetricProjectionReducer.Apply(s, e), "SystemMetricDefinedEvent",     envelope, cancellationToken),
            SystemMetricDeprecatedEventSchema e  => Project(e.AggregateId, s => SystemMetricProjectionReducer.Apply(s, e), "SystemMetricDeprecatedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SystemMetricProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(SystemMetricDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemMetricProjectionReducer.Apply(s, e), "SystemMetricDefinedEvent", null, ct);

    public Task HandleAsync(SystemMetricDeprecatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemMetricProjectionReducer.Apply(s, e), "SystemMetricDeprecatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<SystemMetricReadModel, SystemMetricReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new SystemMetricReadModel { MetricId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventTypeName,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
