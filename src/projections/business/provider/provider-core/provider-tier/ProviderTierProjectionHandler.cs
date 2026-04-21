using Whycespace.Projections.Business.Provider.ProviderCore.ProviderTier.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Provider.ProviderCore.ProviderTier;

public sealed class ProviderTierProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ProviderTierCreatedEventSchema>,
    IProjectionHandler<ProviderTierUpdatedEventSchema>,
    IProjectionHandler<ProviderTierActivatedEventSchema>,
    IProjectionHandler<ProviderTierArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ProviderTierReadModel> _store;

    public ProviderTierProjectionHandler(PostgresProjectionStore<ProviderTierReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ProviderTierCreatedEventSchema e   => Project(e.AggregateId, s => ProviderTierProjectionReducer.Apply(s, e), "ProviderTierCreatedEvent",   envelope, cancellationToken),
            ProviderTierUpdatedEventSchema e   => Project(e.AggregateId, s => ProviderTierProjectionReducer.Apply(s, e), "ProviderTierUpdatedEvent",   envelope, cancellationToken),
            ProviderTierActivatedEventSchema e => Project(e.AggregateId, s => ProviderTierProjectionReducer.Apply(s, e), "ProviderTierActivatedEvent", envelope, cancellationToken),
            ProviderTierArchivedEventSchema e  => Project(e.AggregateId, s => ProviderTierProjectionReducer.Apply(s, e), "ProviderTierArchivedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ProviderTierProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ProviderTierCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderTierProjectionReducer.Apply(s, e), "ProviderTierCreatedEvent", null, ct);

    public Task HandleAsync(ProviderTierUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderTierProjectionReducer.Apply(s, e), "ProviderTierUpdatedEvent", null, ct);

    public Task HandleAsync(ProviderTierActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderTierProjectionReducer.Apply(s, e), "ProviderTierActivatedEvent", null, ct);

    public Task HandleAsync(ProviderTierArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderTierProjectionReducer.Apply(s, e), "ProviderTierArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ProviderTierReadModel, ProviderTierReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ProviderTierReadModel { ProviderTierId = aggregateId };
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
