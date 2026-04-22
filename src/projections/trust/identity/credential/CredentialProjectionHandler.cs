using Whycespace.Projections.Shared;
using Whycespace.Projections.Trust.Identity.Credential.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Trust.Identity.Credential;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Trust.Identity.Credential;

namespace Whycespace.Projections.Trust.Identity.Credential;

public sealed class CredentialProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<CredentialIssuedEventSchema>,
    IProjectionHandler<CredentialActivatedEventSchema>,
    IProjectionHandler<CredentialRevokedEventSchema>
{
    private readonly PostgresProjectionStore<CredentialReadModel> _store;

    public CredentialProjectionHandler(PostgresProjectionStore<CredentialReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            CredentialIssuedEventSchema e => Project(e.AggregateId, s => CredentialProjectionReducer.Apply(s, e), "CredentialIssuedEvent", envelope, cancellationToken),
            CredentialActivatedEventSchema e => Project(e.AggregateId, s => CredentialProjectionReducer.Apply(s, e), "CredentialActivatedEvent", envelope, cancellationToken),
            CredentialRevokedEventSchema e => Project(e.AggregateId, s => CredentialProjectionReducer.Apply(s, e), "CredentialRevokedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CredentialProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(CredentialIssuedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CredentialProjectionReducer.Apply(s, e), "CredentialIssuedEvent", null, ct);

    public Task HandleAsync(CredentialActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CredentialProjectionReducer.Apply(s, e), "CredentialActivatedEvent", null, ct);

    public Task HandleAsync(CredentialRevokedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CredentialProjectionReducer.Apply(s, e), "CredentialRevokedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CredentialReadModel, CredentialReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CredentialReadModel { CredentialId = aggregateId };
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
