using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Cluster.Provider.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Cluster.Provider;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Cluster.Provider;

namespace Whycespace.Projections.Structural.Cluster.Provider;

public sealed class ProviderProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ProviderRegisteredEventSchema>,
    IProjectionHandler<ProviderAttachedEventSchema>,
    IProjectionHandler<ProviderBindingValidatedEventSchema>,
    IProjectionHandler<ProviderActivatedEventSchema>,
    IProjectionHandler<ProviderSuspendedEventSchema>,
    IProjectionHandler<ProviderReactivatedEventSchema>,
    IProjectionHandler<ProviderRetiredEventSchema>
{
    private readonly PostgresProjectionStore<ProviderReadModel> _store;

    public ProviderProjectionHandler(PostgresProjectionStore<ProviderReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ProviderRegisteredEventSchema e => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, envelope.Timestamp), "ProviderRegisteredEvent", envelope, cancellationToken),
            ProviderAttachedEventSchema e => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, envelope.Timestamp), "ProviderAttachedEvent", envelope, cancellationToken),
            ProviderBindingValidatedEventSchema e => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, envelope.Timestamp), "ProviderBindingValidatedEvent", envelope, cancellationToken),
            ProviderActivatedEventSchema e => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, envelope.Timestamp), "ProviderActivatedEvent", envelope, cancellationToken),
            ProviderSuspendedEventSchema e => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, envelope.Timestamp), "ProviderSuspendedEvent", envelope, cancellationToken),
            ProviderReactivatedEventSchema e => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, envelope.Timestamp), "ProviderReactivatedEvent", envelope, cancellationToken),
            ProviderRetiredEventSchema e => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, envelope.Timestamp), "ProviderRetiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ProviderProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ProviderRegisteredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ProviderRegisteredEvent", null, ct);
    public Task HandleAsync(ProviderAttachedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ProviderAttachedEvent", null, ct);
    public Task HandleAsync(ProviderBindingValidatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ProviderBindingValidatedEvent", null, ct);
    public Task HandleAsync(ProviderActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ProviderActivatedEvent", null, ct);
    public Task HandleAsync(ProviderSuspendedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ProviderSuspendedEvent", null, ct);
    public Task HandleAsync(ProviderReactivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ProviderReactivatedEvent", null, ct);
    public Task HandleAsync(ProviderRetiredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProviderProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ProviderRetiredEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ProviderReadModel, ProviderReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ProviderReadModel { ProviderId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
