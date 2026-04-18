using Whycespace.Projections.Economic.Enforcement.Lock.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Enforcement.Lock;

public sealed class LockProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SystemLockedEventSchema>,
    IProjectionHandler<SystemUnlockedEventSchema>,
    IProjectionHandler<SystemLockSuspendedEventSchema>,
    IProjectionHandler<SystemLockResumedEventSchema>,
    IProjectionHandler<SystemLockExpiredEventSchema>
{
    private readonly PostgresProjectionStore<LockReadModel> _store;

    public LockProjectionHandler(PostgresProjectionStore<LockReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SystemLockedEventSchema e         => Project(e.AggregateId, s => LockProjectionReducer.Apply(s, e), "SystemLockedEvent", envelope, cancellationToken),
            SystemUnlockedEventSchema e       => Project(e.AggregateId, s => LockProjectionReducer.Apply(s, e), "SystemUnlockedEvent", envelope, cancellationToken),
            SystemLockSuspendedEventSchema e  => Project(e.AggregateId, s => LockProjectionReducer.Apply(s, e), "SystemLockSuspendedEvent", envelope, cancellationToken),
            SystemLockResumedEventSchema e    => Project(e.AggregateId, s => LockProjectionReducer.Apply(s, e), "SystemLockResumedEvent", envelope, cancellationToken),
            SystemLockExpiredEventSchema e    => Project(e.AggregateId, s => LockProjectionReducer.Apply(s, e), "SystemLockExpiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"LockProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(SystemLockedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LockProjectionReducer.Apply(s, e), "SystemLockedEvent", null, ct);

    public Task HandleAsync(SystemUnlockedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LockProjectionReducer.Apply(s, e), "SystemUnlockedEvent", null, ct);

    public Task HandleAsync(SystemLockSuspendedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LockProjectionReducer.Apply(s, e), "SystemLockSuspendedEvent", null, ct);

    public Task HandleAsync(SystemLockResumedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LockProjectionReducer.Apply(s, e), "SystemLockResumedEvent", null, ct);

    public Task HandleAsync(SystemLockExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LockProjectionReducer.Apply(s, e), "SystemLockExpiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<LockReadModel, LockReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new LockReadModel { LockId = aggregateId };
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
