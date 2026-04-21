using Whycespace.Projections.Content.Streaming.DeliveryGovernance.Access.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Access;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Access;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.DeliveryGovernance.Access;

public sealed class StreamAccessProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<StreamAccessGrantedEventSchema>,
    IProjectionHandler<StreamAccessRestrictedEventSchema>,
    IProjectionHandler<StreamAccessUnrestrictedEventSchema>,
    IProjectionHandler<StreamAccessRevokedEventSchema>,
    IProjectionHandler<StreamAccessExpiredEventSchema>
{
    private readonly PostgresProjectionStore<StreamAccessReadModel> _store;

    public StreamAccessProjectionHandler(PostgresProjectionStore<StreamAccessReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            StreamAccessGrantedEventSchema e => Project(e.AggregateId, s => StreamAccessProjectionReducer.Apply(s, e), "StreamAccessGrantedEvent", envelope, cancellationToken),
            StreamAccessRestrictedEventSchema e => Project(e.AggregateId, s => StreamAccessProjectionReducer.Apply(s, e), "StreamAccessRestrictedEvent", envelope, cancellationToken),
            StreamAccessUnrestrictedEventSchema e => Project(e.AggregateId, s => StreamAccessProjectionReducer.Apply(s, e), "StreamAccessUnrestrictedEvent", envelope, cancellationToken),
            StreamAccessRevokedEventSchema e => Project(e.AggregateId, s => StreamAccessProjectionReducer.Apply(s, e), "StreamAccessRevokedEvent", envelope, cancellationToken),
            StreamAccessExpiredEventSchema e => Project(e.AggregateId, s => StreamAccessProjectionReducer.Apply(s, e), "StreamAccessExpiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"StreamAccessProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(StreamAccessGrantedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StreamAccessProjectionReducer.Apply(s, e), "StreamAccessGrantedEvent", null, ct);
    public Task HandleAsync(StreamAccessRestrictedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StreamAccessProjectionReducer.Apply(s, e), "StreamAccessRestrictedEvent", null, ct);
    public Task HandleAsync(StreamAccessUnrestrictedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StreamAccessProjectionReducer.Apply(s, e), "StreamAccessUnrestrictedEvent", null, ct);
    public Task HandleAsync(StreamAccessRevokedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StreamAccessProjectionReducer.Apply(s, e), "StreamAccessRevokedEvent", null, ct);
    public Task HandleAsync(StreamAccessExpiredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StreamAccessProjectionReducer.Apply(s, e), "StreamAccessExpiredEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<StreamAccessReadModel, StreamAccessReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new StreamAccessReadModel { AccessId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
