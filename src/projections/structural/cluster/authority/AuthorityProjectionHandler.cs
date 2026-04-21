using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Cluster.Authority.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Cluster.Authority;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Cluster.Authority;

namespace Whycespace.Projections.Structural.Cluster.Authority;

public sealed class AuthorityProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AuthorityEstablishedEventSchema>,
    IProjectionHandler<AuthorityAttachedEventSchema>,
    IProjectionHandler<AuthorityBindingValidatedEventSchema>,
    IProjectionHandler<AuthorityActivatedEventSchema>,
    IProjectionHandler<AuthorityRevokedEventSchema>,
    IProjectionHandler<AuthoritySuspendedEventSchema>,
    IProjectionHandler<AuthorityReactivatedEventSchema>,
    IProjectionHandler<AuthorityRetiredEventSchema>
{
    private readonly PostgresProjectionStore<AuthorityReadModel> _store;

    public AuthorityProjectionHandler(PostgresProjectionStore<AuthorityReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            AuthorityEstablishedEventSchema e => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, envelope.Timestamp), "AuthorityEstablishedEvent", envelope, cancellationToken),
            AuthorityAttachedEventSchema e => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, envelope.Timestamp), "AuthorityAttachedEvent", envelope, cancellationToken),
            AuthorityBindingValidatedEventSchema e => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, envelope.Timestamp), "AuthorityBindingValidatedEvent", envelope, cancellationToken),
            AuthorityActivatedEventSchema e => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, envelope.Timestamp), "AuthorityActivatedEvent", envelope, cancellationToken),
            AuthorityRevokedEventSchema e => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, envelope.Timestamp), "AuthorityRevokedEvent", envelope, cancellationToken),
            AuthoritySuspendedEventSchema e => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, envelope.Timestamp), "AuthoritySuspendedEvent", envelope, cancellationToken),
            AuthorityReactivatedEventSchema e => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, envelope.Timestamp), "AuthorityReactivatedEvent", envelope, cancellationToken),
            AuthorityRetiredEventSchema e => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, envelope.Timestamp), "AuthorityRetiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AuthorityProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(AuthorityEstablishedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AuthorityEstablishedEvent", null, ct);
    public Task HandleAsync(AuthorityAttachedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AuthorityAttachedEvent", null, ct);
    public Task HandleAsync(AuthorityBindingValidatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AuthorityBindingValidatedEvent", null, ct);
    public Task HandleAsync(AuthorityActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AuthorityActivatedEvent", null, ct);
    public Task HandleAsync(AuthorityRevokedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AuthorityRevokedEvent", null, ct);
    public Task HandleAsync(AuthoritySuspendedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AuthoritySuspendedEvent", null, ct);
    public Task HandleAsync(AuthorityReactivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AuthorityReactivatedEvent", null, ct);
    public Task HandleAsync(AuthorityRetiredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AuthorityProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AuthorityRetiredEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<AuthorityReadModel, AuthorityReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new AuthorityReadModel { AuthorityId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
