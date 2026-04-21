using Whycespace.Projections.Business.Provider.ProviderGovernance.ProviderAgreement.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Provider.ProviderGovernance.ProviderAgreement;

public sealed class ProviderAgreementProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ProviderAgreementCreatedEventSchema>,
    IProjectionHandler<ProviderAgreementActivatedEventSchema>,
    IProjectionHandler<ProviderAgreementSuspendedEventSchema>,
    IProjectionHandler<ProviderAgreementTerminatedEventSchema>
{
    private readonly PostgresProjectionStore<ProviderAgreementReadModel> _store;

    public ProviderAgreementProjectionHandler(PostgresProjectionStore<ProviderAgreementReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ProviderAgreementCreatedEventSchema e    => Project(e.AggregateId, s => ProviderAgreementProjectionReducer.Apply(s, e), "ProviderAgreementCreatedEvent",    envelope, cancellationToken),
            ProviderAgreementActivatedEventSchema e  => Project(e.AggregateId, s => ProviderAgreementProjectionReducer.Apply(s, e), "ProviderAgreementActivatedEvent",  envelope, cancellationToken),
            ProviderAgreementSuspendedEventSchema e  => Project(e.AggregateId, s => ProviderAgreementProjectionReducer.Apply(s, e), "ProviderAgreementSuspendedEvent",  envelope, cancellationToken),
            ProviderAgreementTerminatedEventSchema e => Project(e.AggregateId, s => ProviderAgreementProjectionReducer.Apply(s, e), "ProviderAgreementTerminatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ProviderAgreementProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ProviderAgreementCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderAgreementProjectionReducer.Apply(s, e), "ProviderAgreementCreatedEvent", null, ct);

    public Task HandleAsync(ProviderAgreementActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderAgreementProjectionReducer.Apply(s, e), "ProviderAgreementActivatedEvent", null, ct);

    public Task HandleAsync(ProviderAgreementSuspendedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderAgreementProjectionReducer.Apply(s, e), "ProviderAgreementSuspendedEvent", null, ct);

    public Task HandleAsync(ProviderAgreementTerminatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderAgreementProjectionReducer.Apply(s, e), "ProviderAgreementTerminatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ProviderAgreementReadModel, ProviderAgreementReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ProviderAgreementReadModel { ProviderAgreementId = aggregateId };
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
