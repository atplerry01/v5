using Whycespace.Projections.Business.Service.ServiceCore.ServiceOption.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Service.ServiceCore.ServiceOption;

public sealed class ServiceOptionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ServiceOptionCreatedEventSchema>,
    IProjectionHandler<ServiceOptionUpdatedEventSchema>,
    IProjectionHandler<ServiceOptionActivatedEventSchema>,
    IProjectionHandler<ServiceOptionArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ServiceOptionReadModel> _store;

    public ServiceOptionProjectionHandler(PostgresProjectionStore<ServiceOptionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ServiceOptionCreatedEventSchema e   => Project(e.AggregateId, s => ServiceOptionProjectionReducer.Apply(s, e), "ServiceOptionCreatedEvent",   envelope, cancellationToken),
            ServiceOptionUpdatedEventSchema e   => Project(e.AggregateId, s => ServiceOptionProjectionReducer.Apply(s, e), "ServiceOptionUpdatedEvent",   envelope, cancellationToken),
            ServiceOptionActivatedEventSchema e => Project(e.AggregateId, s => ServiceOptionProjectionReducer.Apply(s, e), "ServiceOptionActivatedEvent", envelope, cancellationToken),
            ServiceOptionArchivedEventSchema e  => Project(e.AggregateId, s => ServiceOptionProjectionReducer.Apply(s, e), "ServiceOptionArchivedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ServiceOptionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ServiceOptionCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceOptionProjectionReducer.Apply(s, e), "ServiceOptionCreatedEvent", null, ct);

    public Task HandleAsync(ServiceOptionUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceOptionProjectionReducer.Apply(s, e), "ServiceOptionUpdatedEvent", null, ct);

    public Task HandleAsync(ServiceOptionActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceOptionProjectionReducer.Apply(s, e), "ServiceOptionActivatedEvent", null, ct);

    public Task HandleAsync(ServiceOptionArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceOptionProjectionReducer.Apply(s, e), "ServiceOptionArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ServiceOptionReadModel, ServiceOptionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ServiceOptionReadModel { ServiceOptionId = aggregateId };
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
