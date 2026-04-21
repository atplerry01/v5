using Whycespace.Projections.Content.Streaming.StreamCore.Channel.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Channel;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Channel;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.StreamCore.Channel;

public sealed class ChannelProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ChannelCreatedEventSchema>,
    IProjectionHandler<ChannelRenamedEventSchema>,
    IProjectionHandler<ChannelEnabledEventSchema>,
    IProjectionHandler<ChannelDisabledEventSchema>,
    IProjectionHandler<ChannelArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ChannelReadModel> _store;

    public ChannelProjectionHandler(PostgresProjectionStore<ChannelReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ChannelCreatedEventSchema e => Project(e.AggregateId, s => ChannelProjectionReducer.Apply(s, e), "ChannelCreatedEvent", envelope, cancellationToken),
            ChannelRenamedEventSchema e => Project(e.AggregateId, s => ChannelProjectionReducer.Apply(s, e), "ChannelRenamedEvent", envelope, cancellationToken),
            ChannelEnabledEventSchema e => Project(e.AggregateId, s => ChannelProjectionReducer.Apply(s, e), "ChannelEnabledEvent", envelope, cancellationToken),
            ChannelDisabledEventSchema e => Project(e.AggregateId, s => ChannelProjectionReducer.Apply(s, e), "ChannelDisabledEvent", envelope, cancellationToken),
            ChannelArchivedEventSchema e => Project(e.AggregateId, s => ChannelProjectionReducer.Apply(s, e), "ChannelArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ChannelProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ChannelCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ChannelProjectionReducer.Apply(s, e), "ChannelCreatedEvent", null, ct);
    public Task HandleAsync(ChannelRenamedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ChannelProjectionReducer.Apply(s, e), "ChannelRenamedEvent", null, ct);
    public Task HandleAsync(ChannelEnabledEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ChannelProjectionReducer.Apply(s, e), "ChannelEnabledEvent", null, ct);
    public Task HandleAsync(ChannelDisabledEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ChannelProjectionReducer.Apply(s, e), "ChannelDisabledEvent", null, ct);
    public Task HandleAsync(ChannelArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ChannelProjectionReducer.Apply(s, e), "ChannelArchivedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ChannelReadModel, ChannelReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ChannelReadModel { ChannelId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
