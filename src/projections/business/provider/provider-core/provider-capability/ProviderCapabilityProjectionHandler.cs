using Whycespace.Projections.Business.Provider.ProviderCore.ProviderCapability.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Provider.ProviderCore.ProviderCapability;

public sealed class ProviderCapabilityProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ProviderCapabilityCreatedEventSchema>,
    IProjectionHandler<ProviderCapabilityUpdatedEventSchema>,
    IProjectionHandler<ProviderCapabilityActivatedEventSchema>,
    IProjectionHandler<ProviderCapabilityArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ProviderCapabilityReadModel> _store;

    public ProviderCapabilityProjectionHandler(PostgresProjectionStore<ProviderCapabilityReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ProviderCapabilityCreatedEventSchema e   => Project(e.AggregateId, s => ProviderCapabilityProjectionReducer.Apply(s, e), "ProviderCapabilityCreatedEvent",   envelope, cancellationToken),
            ProviderCapabilityUpdatedEventSchema e   => Project(e.AggregateId, s => ProviderCapabilityProjectionReducer.Apply(s, e), "ProviderCapabilityUpdatedEvent",   envelope, cancellationToken),
            ProviderCapabilityActivatedEventSchema e => Project(e.AggregateId, s => ProviderCapabilityProjectionReducer.Apply(s, e), "ProviderCapabilityActivatedEvent", envelope, cancellationToken),
            ProviderCapabilityArchivedEventSchema e  => Project(e.AggregateId, s => ProviderCapabilityProjectionReducer.Apply(s, e), "ProviderCapabilityArchivedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ProviderCapabilityProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ProviderCapabilityCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderCapabilityProjectionReducer.Apply(s, e), "ProviderCapabilityCreatedEvent", null, ct);

    public Task HandleAsync(ProviderCapabilityUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderCapabilityProjectionReducer.Apply(s, e), "ProviderCapabilityUpdatedEvent", null, ct);

    public Task HandleAsync(ProviderCapabilityActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderCapabilityProjectionReducer.Apply(s, e), "ProviderCapabilityActivatedEvent", null, ct);

    public Task HandleAsync(ProviderCapabilityArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderCapabilityProjectionReducer.Apply(s, e), "ProviderCapabilityArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ProviderCapabilityReadModel, ProviderCapabilityReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ProviderCapabilityReadModel { ProviderCapabilityId = aggregateId };
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
