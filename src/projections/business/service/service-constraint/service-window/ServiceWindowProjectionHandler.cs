using Whycespace.Projections.Business.Service.ServiceConstraint.ServiceWindow.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Service.ServiceConstraint.ServiceWindow;

public sealed class ServiceWindowProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ServiceWindowCreatedEventSchema>,
    IProjectionHandler<ServiceWindowUpdatedEventSchema>,
    IProjectionHandler<ServiceWindowActivatedEventSchema>,
    IProjectionHandler<ServiceWindowArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ServiceWindowReadModel> _store;

    public ServiceWindowProjectionHandler(PostgresProjectionStore<ServiceWindowReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ServiceWindowCreatedEventSchema e   => Project(e.AggregateId, s => ServiceWindowProjectionReducer.Apply(s, e), "ServiceWindowCreatedEvent",   envelope, cancellationToken),
            ServiceWindowUpdatedEventSchema e   => Project(e.AggregateId, s => ServiceWindowProjectionReducer.Apply(s, e), "ServiceWindowUpdatedEvent",   envelope, cancellationToken),
            ServiceWindowActivatedEventSchema e => Project(e.AggregateId, s => ServiceWindowProjectionReducer.Apply(s, e), "ServiceWindowActivatedEvent", envelope, cancellationToken),
            ServiceWindowArchivedEventSchema e  => Project(e.AggregateId, s => ServiceWindowProjectionReducer.Apply(s, e), "ServiceWindowArchivedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ServiceWindowProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ServiceWindowCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceWindowProjectionReducer.Apply(s, e), "ServiceWindowCreatedEvent", null, ct);

    public Task HandleAsync(ServiceWindowUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceWindowProjectionReducer.Apply(s, e), "ServiceWindowUpdatedEvent", null, ct);

    public Task HandleAsync(ServiceWindowActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceWindowProjectionReducer.Apply(s, e), "ServiceWindowActivatedEvent", null, ct);

    public Task HandleAsync(ServiceWindowArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceWindowProjectionReducer.Apply(s, e), "ServiceWindowArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ServiceWindowReadModel, ServiceWindowReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ServiceWindowReadModel { ServiceWindowId = aggregateId };
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
