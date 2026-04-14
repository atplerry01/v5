using Whycespace.Projections.Economic.Transaction.Expense.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Transaction.Expense;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Expense;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Transaction.Expense;

public sealed class ExpenseProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ExpenseCreatedEventSchema>,
    IProjectionHandler<ExpenseRecordedEventSchema>,
    IProjectionHandler<ExpenseCancelledEventSchema>
{
    private readonly PostgresProjectionStore<ExpenseReadModel> _store;

    public ExpenseProjectionHandler(PostgresProjectionStore<ExpenseReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ExpenseCreatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            ExpenseRecordedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            ExpenseCancelledEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ExpenseProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(ExpenseCreatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(ExpenseRecordedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(ExpenseCancelledEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(ExpenseCreatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new ExpenseReadModel { ExpenseId = e.AggregateId };
        state = ExpenseProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "ExpenseCreatedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(ExpenseRecordedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new ExpenseReadModel { ExpenseId = e.AggregateId };
        state = ExpenseProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "ExpenseRecordedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(ExpenseCancelledEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new ExpenseReadModel { ExpenseId = e.AggregateId };
        state = ExpenseProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "ExpenseCancelledEvent", eventId, eventVersion, correlationId, ct);
    }
}
