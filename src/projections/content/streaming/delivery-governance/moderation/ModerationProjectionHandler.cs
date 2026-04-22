using Whycespace.Projections.Content.Streaming.DeliveryGovernance.Moderation.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.DeliveryGovernance.Moderation;

public sealed class ModerationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<StreamFlaggedEventSchema>,
    IProjectionHandler<ModerationAssignedEventSchema>,
    IProjectionHandler<ModerationDecidedEventSchema>,
    IProjectionHandler<ModerationOverturnedEventSchema>
{
    private readonly PostgresProjectionStore<ModerationReadModel> _store;

    public ModerationProjectionHandler(PostgresProjectionStore<ModerationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            StreamFlaggedEventSchema e => Project(e.AggregateId, s => ModerationProjectionReducer.Apply(s, e), "StreamFlaggedEvent", envelope, cancellationToken),
            ModerationAssignedEventSchema e => Project(e.AggregateId, s => ModerationProjectionReducer.Apply(s, e), "ModerationAssignedEvent", envelope, cancellationToken),
            ModerationDecidedEventSchema e => Project(e.AggregateId, s => ModerationProjectionReducer.Apply(s, e), "ModerationDecidedEvent", envelope, cancellationToken),
            ModerationOverturnedEventSchema e => Project(e.AggregateId, s => ModerationProjectionReducer.Apply(s, e), "ModerationOverturnedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ModerationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(StreamFlaggedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ModerationProjectionReducer.Apply(s, e), "StreamFlaggedEvent", null, ct);
    public Task HandleAsync(ModerationAssignedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ModerationProjectionReducer.Apply(s, e), "ModerationAssignedEvent", null, ct);
    public Task HandleAsync(ModerationDecidedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ModerationProjectionReducer.Apply(s, e), "ModerationDecidedEvent", null, ct);
    public Task HandleAsync(ModerationOverturnedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ModerationProjectionReducer.Apply(s, e), "ModerationOverturnedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ModerationReadModel, ModerationReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ModerationReadModel { ModerationId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
