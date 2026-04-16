using Whycespace.Projections.Economic.Transaction.Instruction.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Transaction.Instruction;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Instruction;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Transaction.Instruction;

public sealed class InstructionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<TransactionInstructionCreatedEventSchema>,
    IProjectionHandler<TransactionInstructionExecutedEventSchema>,
    IProjectionHandler<TransactionInstructionCancelledEventSchema>
{
    private readonly PostgresProjectionStore<InstructionReadModel> _store;

    public InstructionProjectionHandler(PostgresProjectionStore<InstructionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            TransactionInstructionCreatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            TransactionInstructionExecutedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            TransactionInstructionCancelledEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"InstructionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(TransactionInstructionCreatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(TransactionInstructionExecutedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(TransactionInstructionCancelledEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(TransactionInstructionCreatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new InstructionReadModel { InstructionId = e.AggregateId };
        state = InstructionProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "TransactionInstructionCreatedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(TransactionInstructionExecutedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new InstructionReadModel { InstructionId = e.AggregateId };
        state = InstructionProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "TransactionInstructionExecutedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(TransactionInstructionCancelledEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new InstructionReadModel { InstructionId = e.AggregateId };
        state = InstructionProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "TransactionInstructionCancelledEvent", eventId, eventVersion, correlationId, ct);
    }
}
