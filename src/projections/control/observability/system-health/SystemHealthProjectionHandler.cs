using Whycespace.Projections.Control.Observability.SystemHealth.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Observability.SystemHealth;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Observability.SystemHealth;

public sealed class SystemHealthProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SystemHealthEvaluatedEventSchema>,
    IProjectionHandler<SystemHealthDegradedEventSchema>,
    IProjectionHandler<SystemHealthRestoredEventSchema>
{
    private readonly PostgresProjectionStore<SystemHealthReadModel> _store;

    public SystemHealthProjectionHandler(PostgresProjectionStore<SystemHealthReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SystemHealthEvaluatedEventSchema e  => Project(e.AggregateId, s => SystemHealthProjectionReducer.Apply(s, e), "SystemHealthEvaluatedEvent",  envelope, cancellationToken),
            SystemHealthDegradedEventSchema e   => Project(e.AggregateId, s => SystemHealthProjectionReducer.Apply(s, e), "SystemHealthDegradedEvent",   envelope, cancellationToken),
            SystemHealthRestoredEventSchema e   => Project(e.AggregateId, s => SystemHealthProjectionReducer.Apply(s, e), "SystemHealthRestoredEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SystemHealthProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(SystemHealthEvaluatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemHealthProjectionReducer.Apply(s, e), "SystemHealthEvaluatedEvent", null, ct);

    public Task HandleAsync(SystemHealthDegradedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemHealthProjectionReducer.Apply(s, e), "SystemHealthDegradedEvent", null, ct);

    public Task HandleAsync(SystemHealthRestoredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemHealthProjectionReducer.Apply(s, e), "SystemHealthRestoredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<SystemHealthReadModel, SystemHealthReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new SystemHealthReadModel { HealthId = aggregateId };
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
