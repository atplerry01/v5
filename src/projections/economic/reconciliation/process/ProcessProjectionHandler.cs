using Whycespace.Projections.Economic.Reconciliation.Process.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Reconciliation.Process;

public sealed class ProcessProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ReconciliationTriggeredEventSchema>,
    IProjectionHandler<ReconciliationMatchedEventSchema>,
    IProjectionHandler<ReconciliationMismatchedEventSchema>,
    IProjectionHandler<ReconciliationResolvedEventSchema>
{
    private readonly PostgresProjectionStore<ProcessReadModel> _store;

    public ProcessProjectionHandler(PostgresProjectionStore<ProcessReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ReconciliationTriggeredEventSchema  e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            ReconciliationMatchedEventSchema    e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            ReconciliationMismatchedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            ReconciliationResolvedEventSchema   e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ProcessProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ReconciliationTriggeredEventSchema e, CancellationToken ct = default)
        => Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public Task HandleAsync(ReconciliationMatchedEventSchema e, CancellationToken ct = default)
        => Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public Task HandleAsync(ReconciliationMismatchedEventSchema e, CancellationToken ct = default)
        => Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public Task HandleAsync(ReconciliationResolvedEventSchema e, CancellationToken ct = default)
        => Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(ReconciliationTriggeredEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.ProcessId, ct) ?? new ProcessReadModel { ProcessId = e.ProcessId };
        state = ProcessProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.ProcessId, state, "ReconciliationTriggeredEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(ReconciliationMatchedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.ProcessId, ct) ?? new ProcessReadModel { ProcessId = e.ProcessId };
        state = ProcessProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.ProcessId, state, "ReconciliationMatchedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(ReconciliationMismatchedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.ProcessId, ct) ?? new ProcessReadModel { ProcessId = e.ProcessId };
        state = ProcessProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.ProcessId, state, "ReconciliationMismatchedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(ReconciliationResolvedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.ProcessId, ct) ?? new ProcessReadModel { ProcessId = e.ProcessId };
        state = ProcessProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.ProcessId, state, "ReconciliationResolvedEvent", eventId, eventVersion, correlationId, ct);
    }
}
