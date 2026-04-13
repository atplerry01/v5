using Whyce.Projections.Operational.Sandbox.Kanban.Reducer;
using Whyce.Projections.Shared;
using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Events.Operational.Sandbox.Kanban;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Operational.Sandbox.Kanban;
using Whyce.Shared.Contracts.Projection;

namespace Whyce.Projections.Operational.Sandbox.Kanban.Card;

public sealed class KanbanCardProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<KanbanCardCreatedEventSchema>,
    IProjectionHandler<KanbanCardMovedEventSchema>,
    IProjectionHandler<KanbanCardReorderedEventSchema>,
    IProjectionHandler<KanbanCardCompletedEventSchema>,
    IProjectionHandler<KanbanCardUpdatedEventSchema>
{
    private readonly PostgresProjectionStore<KanbanBoardReadModel> _store;

    public KanbanCardProjectionHandler(PostgresProjectionStore<KanbanBoardReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            KanbanCardCreatedEventSchema e => ProjectCreate(e, envelope.EventId, envelope.CorrelationId, cancellationToken),
            KanbanCardMovedEventSchema e => ProjectMutate(e.AggregateId, s => KanbanProjectionReducer.Apply(s, e), "KanbanCardMovedEvent", envelope.EventId, envelope.CorrelationId, cancellationToken),
            KanbanCardReorderedEventSchema e => ProjectMutate(e.AggregateId, s => KanbanProjectionReducer.Apply(s, e), "KanbanCardReorderedEvent", envelope.EventId, envelope.CorrelationId, cancellationToken),
            KanbanCardCompletedEventSchema e => ProjectMutate(e.AggregateId, s => KanbanProjectionReducer.Apply(s, e), "KanbanCardCompletedEvent", envelope.EventId, envelope.CorrelationId, cancellationToken),
            KanbanCardUpdatedEventSchema e => ProjectMutate(e.AggregateId, s => KanbanProjectionReducer.Apply(s, e), "KanbanCardUpdatedEvent", envelope.EventId, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"KanbanCardProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(KanbanCardCreatedEventSchema e, CancellationToken ct = default) => await ProjectCreate(e, Guid.Empty, Guid.Empty, ct);
    public async Task HandleAsync(KanbanCardMovedEventSchema e, CancellationToken ct = default) => await ProjectMutate(e.AggregateId, s => KanbanProjectionReducer.Apply(s, e), "KanbanCardMovedEvent", Guid.Empty, Guid.Empty, ct);
    public async Task HandleAsync(KanbanCardReorderedEventSchema e, CancellationToken ct = default) => await ProjectMutate(e.AggregateId, s => KanbanProjectionReducer.Apply(s, e), "KanbanCardReorderedEvent", Guid.Empty, Guid.Empty, ct);
    public async Task HandleAsync(KanbanCardCompletedEventSchema e, CancellationToken ct = default) => await ProjectMutate(e.AggregateId, s => KanbanProjectionReducer.Apply(s, e), "KanbanCardCompletedEvent", Guid.Empty, Guid.Empty, ct);
    public async Task HandleAsync(KanbanCardUpdatedEventSchema e, CancellationToken ct = default) => await ProjectMutate(e.AggregateId, s => KanbanProjectionReducer.Apply(s, e), "KanbanCardUpdatedEvent", Guid.Empty, Guid.Empty, ct);

    private async Task ProjectCreate(KanbanCardCreatedEventSchema e, Guid eventId, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new KanbanBoardReadModel { BoardId = e.AggregateId };
        state = KanbanProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "KanbanCardCreatedEvent", eventId, correlationId, ct);
    }

    private async Task ProjectMutate(Guid aggregateId, Func<KanbanBoardReadModel?, KanbanBoardReadModel?> reduce, string eventType, Guid eventId, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct);
        state = reduce(state);
        if (state is null) return;
        await _store.UpsertAsync(aggregateId, state, eventType, eventId, correlationId, ct);
    }
}
