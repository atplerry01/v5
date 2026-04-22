using Whycespace.Projections.Control.Scheduling.SystemJob.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Scheduling.SystemJob;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Scheduling.SystemJob;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Scheduling.SystemJob;

public sealed class SystemJobProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SystemJobDefinedEventSchema>,
    IProjectionHandler<SystemJobDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<SystemJobReadModel> _store;

    public SystemJobProjectionHandler(PostgresProjectionStore<SystemJobReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SystemJobDefinedEventSchema e     => Project(e.AggregateId, s => SystemJobProjectionReducer.Apply(s, e), "SystemJobDefinedEvent",     envelope, cancellationToken),
            SystemJobDeprecatedEventSchema e  => Project(e.AggregateId, s => SystemJobProjectionReducer.Apply(s, e), "SystemJobDeprecatedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SystemJobProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(SystemJobDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemJobProjectionReducer.Apply(s, e), "SystemJobDefinedEvent", null, ct);

    public Task HandleAsync(SystemJobDeprecatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemJobProjectionReducer.Apply(s, e), "SystemJobDeprecatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<SystemJobReadModel, SystemJobReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new SystemJobReadModel { JobId = aggregateId };
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
