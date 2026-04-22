using Whycespace.Projections.Control.Observability.SystemAlert.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Observability.SystemAlert;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Observability.SystemAlert;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Observability.SystemAlert;

public sealed class SystemAlertProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SystemAlertDefinedEventSchema>,
    IProjectionHandler<SystemAlertRetiredEventSchema>
{
    private readonly PostgresProjectionStore<SystemAlertReadModel> _store;

    public SystemAlertProjectionHandler(PostgresProjectionStore<SystemAlertReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SystemAlertDefinedEventSchema e  => Project(e.AggregateId, s => SystemAlertProjectionReducer.Apply(s, e), "SystemAlertDefinedEvent",  envelope, cancellationToken),
            SystemAlertRetiredEventSchema e  => Project(e.AggregateId, s => SystemAlertProjectionReducer.Apply(s, e), "SystemAlertRetiredEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SystemAlertProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(SystemAlertDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemAlertProjectionReducer.Apply(s, e), "SystemAlertDefinedEvent", null, ct);

    public Task HandleAsync(SystemAlertRetiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemAlertProjectionReducer.Apply(s, e), "SystemAlertRetiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<SystemAlertReadModel, SystemAlertReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new SystemAlertReadModel { AlertId = aggregateId };
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
