using Whycespace.Projections.Business.Agreement.PartyGovernance.Signature.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.PartyGovernance.Signature;

public sealed class SignatureProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SignatureCreatedEventSchema>,
    IProjectionHandler<SignatureSignedEventSchema>,
    IProjectionHandler<SignatureRevokedEventSchema>
{
    private readonly PostgresProjectionStore<SignatureReadModel> _store;

    public SignatureProjectionHandler(PostgresProjectionStore<SignatureReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SignatureCreatedEventSchema e => Project(e.AggregateId, s => SignatureProjectionReducer.Apply(s, e), "SignatureCreatedEvent", envelope, cancellationToken),
            SignatureSignedEventSchema e  => Project(e.AggregateId, s => SignatureProjectionReducer.Apply(s, e), "SignatureSignedEvent",  envelope, cancellationToken),
            SignatureRevokedEventSchema e => Project(e.AggregateId, s => SignatureProjectionReducer.Apply(s, e), "SignatureRevokedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SignatureProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(SignatureCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SignatureProjectionReducer.Apply(s, e), "SignatureCreatedEvent", null, ct);

    public Task HandleAsync(SignatureSignedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SignatureProjectionReducer.Apply(s, e), "SignatureSignedEvent", null, ct);

    public Task HandleAsync(SignatureRevokedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SignatureProjectionReducer.Apply(s, e), "SignatureRevokedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<SignatureReadModel, SignatureReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new SignatureReadModel { SignatureId = aggregateId };
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
