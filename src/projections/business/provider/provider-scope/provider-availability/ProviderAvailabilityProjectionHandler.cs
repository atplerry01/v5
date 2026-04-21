using Whycespace.Projections.Business.Provider.ProviderScope.ProviderAvailability.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Provider.ProviderScope.ProviderAvailability;

public sealed class ProviderAvailabilityProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ProviderAvailabilityCreatedEventSchema>,
    IProjectionHandler<ProviderAvailabilityUpdatedEventSchema>,
    IProjectionHandler<ProviderAvailabilityActivatedEventSchema>,
    IProjectionHandler<ProviderAvailabilityArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ProviderAvailabilityReadModel> _store;

    public ProviderAvailabilityProjectionHandler(PostgresProjectionStore<ProviderAvailabilityReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ProviderAvailabilityCreatedEventSchema e   => Project(e.AggregateId, s => ProviderAvailabilityProjectionReducer.Apply(s, e), "ProviderAvailabilityCreatedEvent",   envelope, cancellationToken),
            ProviderAvailabilityUpdatedEventSchema e   => Project(e.AggregateId, s => ProviderAvailabilityProjectionReducer.Apply(s, e), "ProviderAvailabilityUpdatedEvent",   envelope, cancellationToken),
            ProviderAvailabilityActivatedEventSchema e => Project(e.AggregateId, s => ProviderAvailabilityProjectionReducer.Apply(s, e), "ProviderAvailabilityActivatedEvent", envelope, cancellationToken),
            ProviderAvailabilityArchivedEventSchema e  => Project(e.AggregateId, s => ProviderAvailabilityProjectionReducer.Apply(s, e), "ProviderAvailabilityArchivedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ProviderAvailabilityProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ProviderAvailabilityCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderAvailabilityProjectionReducer.Apply(s, e), "ProviderAvailabilityCreatedEvent", null, ct);

    public Task HandleAsync(ProviderAvailabilityUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderAvailabilityProjectionReducer.Apply(s, e), "ProviderAvailabilityUpdatedEvent", null, ct);

    public Task HandleAsync(ProviderAvailabilityActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderAvailabilityProjectionReducer.Apply(s, e), "ProviderAvailabilityActivatedEvent", null, ct);

    public Task HandleAsync(ProviderAvailabilityArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderAvailabilityProjectionReducer.Apply(s, e), "ProviderAvailabilityArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ProviderAvailabilityReadModel, ProviderAvailabilityReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ProviderAvailabilityReadModel { ProviderAvailabilityId = aggregateId };
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
