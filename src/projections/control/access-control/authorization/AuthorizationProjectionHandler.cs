using Whycespace.Projections.Control.AccessControl.Authorization.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.AccessControl.Authorization;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.Authorization;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.AccessControl.Authorization;

public sealed class AuthorizationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AuthorizationGrantedEventSchema>,
    IProjectionHandler<AuthorizationRevokedEventSchema>
{
    private readonly PostgresProjectionStore<AuthorizationReadModel> _store;

    public AuthorizationProjectionHandler(PostgresProjectionStore<AuthorizationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AuthorizationGrantedEventSchema e => Project(e.AggregateId, s => AuthorizationProjectionReducer.Apply(s, e), "AuthorizationGrantedEvent", envelope, cancellationToken),
            AuthorizationRevokedEventSchema e => Project(e.AggregateId, s => AuthorizationProjectionReducer.Apply(s, e), "AuthorizationRevokedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AuthorizationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AuthorizationGrantedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuthorizationProjectionReducer.Apply(s, e), "AuthorizationGrantedEvent", null, ct);

    public Task HandleAsync(AuthorizationRevokedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuthorizationProjectionReducer.Apply(s, e), "AuthorizationRevokedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AuthorizationReadModel, AuthorizationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AuthorizationReadModel { AuthorizationId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventTypeName,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
