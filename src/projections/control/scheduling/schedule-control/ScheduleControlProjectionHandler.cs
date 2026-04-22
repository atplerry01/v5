using Whycespace.Projections.Control.Scheduling.ScheduleControl.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Scheduling.ScheduleControl;

public sealed class ScheduleControlProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ScheduleControlDefinedEventSchema>,
    IProjectionHandler<ScheduleControlSuspendedEventSchema>,
    IProjectionHandler<ScheduleControlResumedEventSchema>,
    IProjectionHandler<ScheduleControlRetiredEventSchema>
{
    private readonly PostgresProjectionStore<ScheduleControlReadModel> _store;

    public ScheduleControlProjectionHandler(PostgresProjectionStore<ScheduleControlReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ScheduleControlDefinedEventSchema e   => Project(e.AggregateId, s => ScheduleControlProjectionReducer.Apply(s, e), "ScheduleControlDefinedEvent",   envelope, cancellationToken),
            ScheduleControlSuspendedEventSchema e => Project(e.AggregateId, s => ScheduleControlProjectionReducer.Apply(s, e), "ScheduleControlSuspendedEvent", envelope, cancellationToken),
            ScheduleControlResumedEventSchema e   => Project(e.AggregateId, s => ScheduleControlProjectionReducer.Apply(s, e), "ScheduleControlResumedEvent",   envelope, cancellationToken),
            ScheduleControlRetiredEventSchema e   => Project(e.AggregateId, s => ScheduleControlProjectionReducer.Apply(s, e), "ScheduleControlRetiredEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ScheduleControlProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ScheduleControlDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ScheduleControlProjectionReducer.Apply(s, e), "ScheduleControlDefinedEvent", null, ct);

    public Task HandleAsync(ScheduleControlSuspendedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ScheduleControlProjectionReducer.Apply(s, e), "ScheduleControlSuspendedEvent", null, ct);

    public Task HandleAsync(ScheduleControlResumedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ScheduleControlProjectionReducer.Apply(s, e), "ScheduleControlResumedEvent", null, ct);

    public Task HandleAsync(ScheduleControlRetiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ScheduleControlProjectionReducer.Apply(s, e), "ScheduleControlRetiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ScheduleControlReadModel, ScheduleControlReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ScheduleControlReadModel { ScheduleId = aggregateId };
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
