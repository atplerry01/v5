using Whycespace.Projections.Business.Offering.CatalogCore.ServiceOffering.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Offering.CatalogCore.ServiceOffering;

public sealed class ServiceOfferingProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ServiceOfferingCreatedEventSchema>,
    IProjectionHandler<ServiceOfferingUpdatedEventSchema>,
    IProjectionHandler<ServiceOfferingActivatedEventSchema>,
    IProjectionHandler<ServiceOfferingArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ServiceOfferingReadModel> _store;

    public ServiceOfferingProjectionHandler(PostgresProjectionStore<ServiceOfferingReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ServiceOfferingCreatedEventSchema e    => Project(e.AggregateId, s => ServiceOfferingProjectionReducer.Apply(s, e), "ServiceOfferingCreatedEvent",    envelope, cancellationToken),
            ServiceOfferingUpdatedEventSchema e    => Project(e.AggregateId, s => ServiceOfferingProjectionReducer.Apply(s, e), "ServiceOfferingUpdatedEvent",    envelope, cancellationToken),
            ServiceOfferingActivatedEventSchema e  => Project(e.AggregateId, s => ServiceOfferingProjectionReducer.Apply(s, e), "ServiceOfferingActivatedEvent",  envelope, cancellationToken),
            ServiceOfferingArchivedEventSchema e   => Project(e.AggregateId, s => ServiceOfferingProjectionReducer.Apply(s, e), "ServiceOfferingArchivedEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ServiceOfferingProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ServiceOfferingCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceOfferingProjectionReducer.Apply(s, e), "ServiceOfferingCreatedEvent", null, ct);

    public Task HandleAsync(ServiceOfferingUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceOfferingProjectionReducer.Apply(s, e), "ServiceOfferingUpdatedEvent", null, ct);

    public Task HandleAsync(ServiceOfferingActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceOfferingProjectionReducer.Apply(s, e), "ServiceOfferingActivatedEvent", null, ct);

    public Task HandleAsync(ServiceOfferingArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceOfferingProjectionReducer.Apply(s, e), "ServiceOfferingArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ServiceOfferingReadModel, ServiceOfferingReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ServiceOfferingReadModel { ServiceOfferingId = aggregateId };
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
