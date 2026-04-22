using Whycespace.Projections.Control.Observability.SystemSignal.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Observability.SystemSignal;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Observability.SystemSignal;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Observability.SystemSignal;

public sealed class SystemSignalProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SystemSignalDefinedEventSchema>,
    IProjectionHandler<SystemSignalDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<SystemSignalReadModel> _store;

    public SystemSignalProjectionHandler(PostgresProjectionStore<SystemSignalReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SystemSignalDefinedEventSchema e     => Project(e.AggregateId, s => SystemSignalProjectionReducer.Apply(s, e), "SystemSignalDefinedEvent",     envelope, cancellationToken),
            SystemSignalDeprecatedEventSchema e  => Project(e.AggregateId, s => SystemSignalProjectionReducer.Apply(s, e), "SystemSignalDeprecatedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SystemSignalProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(SystemSignalDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemSignalProjectionReducer.Apply(s, e), "SystemSignalDefinedEvent", null, ct);

    public Task HandleAsync(SystemSignalDeprecatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemSignalProjectionReducer.Apply(s, e), "SystemSignalDeprecatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<SystemSignalReadModel, SystemSignalReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new SystemSignalReadModel { SignalId = aggregateId };
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
