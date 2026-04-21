using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Assignment.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Assignment;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Assignment;

namespace Whycespace.Projections.Structural.Humancapital.Assignment;

public sealed class AssignmentProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AssignmentAssignedEventSchema>
{
    private readonly PostgresProjectionStore<AssignmentReadModel> _store;

    public AssignmentProjectionHandler(PostgresProjectionStore<AssignmentReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            AssignmentAssignedEventSchema e => Project(e.AggregateId, s => AssignmentProjectionReducer.Apply(s, e, envelope.Timestamp), "AssignmentAssignedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AssignmentProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(AssignmentAssignedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AssignmentProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AssignmentAssignedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<AssignmentReadModel, AssignmentReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new AssignmentReadModel { AssignmentId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
