using Whyce.Projections.Operational.Sandbox.Kanban.Reducer;
using Whyce.Projections.Shared;
using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Events.Operational.Sandbox.Kanban;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Operational.Sandbox.Kanban;
using Whyce.Shared.Contracts.Projection;

namespace Whyce.Projections.Operational.Sandbox.Kanban.List;

public sealed class KanbanListProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<KanbanListCreatedEventSchema>
{
    private readonly PostgresProjectionStore<KanbanBoardReadModel> _store;

    public KanbanListProjectionHandler(PostgresProjectionStore<KanbanBoardReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            KanbanListCreatedEventSchema e => Project(e, envelope.EventId, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"KanbanListProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(KanbanListCreatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, Guid.Empty, ct);

    private async Task Project(KanbanListCreatedEventSchema e, Guid eventId, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new KanbanBoardReadModel { BoardId = e.AggregateId };
        state = KanbanProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "KanbanListCreatedEvent", eventId, correlationId, ct);
    }
}
