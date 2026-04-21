using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Operator.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Operator;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Operator;

namespace Whycespace.Projections.Structural.Humancapital.Operator;

public sealed class OperatorProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<OperatorCreatedEventSchema>
{
    private readonly PostgresProjectionStore<OperatorReadModel> _store;

    public OperatorProjectionHandler(PostgresProjectionStore<OperatorReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            OperatorCreatedEventSchema e => Project(e.AggregateId, s => OperatorProjectionReducer.Apply(s, e, envelope.Timestamp), "OperatorCreatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"OperatorProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(OperatorCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => OperatorProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "OperatorCreatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<OperatorReadModel, OperatorReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new OperatorReadModel { OperatorId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
