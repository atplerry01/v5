using Whycespace.Projections.Business.Service.ServiceCore.ServiceLevel.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Service.ServiceCore.ServiceLevel;

public sealed class ServiceLevelProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ServiceLevelCreatedEventSchema>,
    IProjectionHandler<ServiceLevelUpdatedEventSchema>,
    IProjectionHandler<ServiceLevelActivatedEventSchema>,
    IProjectionHandler<ServiceLevelArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ServiceLevelReadModel> _store;

    public ServiceLevelProjectionHandler(PostgresProjectionStore<ServiceLevelReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ServiceLevelCreatedEventSchema e   => Project(e.AggregateId, s => ServiceLevelProjectionReducer.Apply(s, e), "ServiceLevelCreatedEvent",   envelope, cancellationToken),
            ServiceLevelUpdatedEventSchema e   => Project(e.AggregateId, s => ServiceLevelProjectionReducer.Apply(s, e), "ServiceLevelUpdatedEvent",   envelope, cancellationToken),
            ServiceLevelActivatedEventSchema e => Project(e.AggregateId, s => ServiceLevelProjectionReducer.Apply(s, e), "ServiceLevelActivatedEvent", envelope, cancellationToken),
            ServiceLevelArchivedEventSchema e  => Project(e.AggregateId, s => ServiceLevelProjectionReducer.Apply(s, e), "ServiceLevelArchivedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ServiceLevelProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ServiceLevelCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceLevelProjectionReducer.Apply(s, e), "ServiceLevelCreatedEvent", null, ct);

    public Task HandleAsync(ServiceLevelUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceLevelProjectionReducer.Apply(s, e), "ServiceLevelUpdatedEvent", null, ct);

    public Task HandleAsync(ServiceLevelActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceLevelProjectionReducer.Apply(s, e), "ServiceLevelActivatedEvent", null, ct);

    public Task HandleAsync(ServiceLevelArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceLevelProjectionReducer.Apply(s, e), "ServiceLevelArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ServiceLevelReadModel, ServiceLevelReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ServiceLevelReadModel { ServiceLevelId = aggregateId };
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
