using Whycespace.Projections.Business.Service.ServiceCore.ServiceDefinition.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Service.ServiceCore.ServiceDefinition;

public sealed class ServiceDefinitionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ServiceDefinitionCreatedEventSchema>,
    IProjectionHandler<ServiceDefinitionUpdatedEventSchema>,
    IProjectionHandler<ServiceDefinitionActivatedEventSchema>,
    IProjectionHandler<ServiceDefinitionArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ServiceDefinitionReadModel> _store;

    public ServiceDefinitionProjectionHandler(PostgresProjectionStore<ServiceDefinitionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ServiceDefinitionCreatedEventSchema e   => Project(e.AggregateId, s => ServiceDefinitionProjectionReducer.Apply(s, e), "ServiceDefinitionCreatedEvent",   envelope, cancellationToken),
            ServiceDefinitionUpdatedEventSchema e   => Project(e.AggregateId, s => ServiceDefinitionProjectionReducer.Apply(s, e), "ServiceDefinitionUpdatedEvent",   envelope, cancellationToken),
            ServiceDefinitionActivatedEventSchema e => Project(e.AggregateId, s => ServiceDefinitionProjectionReducer.Apply(s, e), "ServiceDefinitionActivatedEvent", envelope, cancellationToken),
            ServiceDefinitionArchivedEventSchema e  => Project(e.AggregateId, s => ServiceDefinitionProjectionReducer.Apply(s, e), "ServiceDefinitionArchivedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ServiceDefinitionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ServiceDefinitionCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceDefinitionProjectionReducer.Apply(s, e), "ServiceDefinitionCreatedEvent", null, ct);

    public Task HandleAsync(ServiceDefinitionUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceDefinitionProjectionReducer.Apply(s, e), "ServiceDefinitionUpdatedEvent", null, ct);

    public Task HandleAsync(ServiceDefinitionActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceDefinitionProjectionReducer.Apply(s, e), "ServiceDefinitionActivatedEvent", null, ct);

    public Task HandleAsync(ServiceDefinitionArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceDefinitionProjectionReducer.Apply(s, e), "ServiceDefinitionArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ServiceDefinitionReadModel, ServiceDefinitionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ServiceDefinitionReadModel { ServiceDefinitionId = aggregateId };
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
