using Whycespace.Projections.Business.Service.ServiceConstraint.ServiceConstraint.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Service.ServiceConstraint.ServiceConstraint;

public sealed class ServiceConstraintProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ServiceConstraintCreatedEventSchema>,
    IProjectionHandler<ServiceConstraintUpdatedEventSchema>,
    IProjectionHandler<ServiceConstraintActivatedEventSchema>,
    IProjectionHandler<ServiceConstraintArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ServiceConstraintReadModel> _store;

    public ServiceConstraintProjectionHandler(PostgresProjectionStore<ServiceConstraintReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ServiceConstraintCreatedEventSchema e   => Project(e.AggregateId, s => ServiceConstraintProjectionReducer.Apply(s, e), "ServiceConstraintCreatedEvent",   envelope, cancellationToken),
            ServiceConstraintUpdatedEventSchema e   => Project(e.AggregateId, s => ServiceConstraintProjectionReducer.Apply(s, e), "ServiceConstraintUpdatedEvent",   envelope, cancellationToken),
            ServiceConstraintActivatedEventSchema e => Project(e.AggregateId, s => ServiceConstraintProjectionReducer.Apply(s, e), "ServiceConstraintActivatedEvent", envelope, cancellationToken),
            ServiceConstraintArchivedEventSchema e  => Project(e.AggregateId, s => ServiceConstraintProjectionReducer.Apply(s, e), "ServiceConstraintArchivedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ServiceConstraintProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ServiceConstraintCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceConstraintProjectionReducer.Apply(s, e), "ServiceConstraintCreatedEvent", null, ct);

    public Task HandleAsync(ServiceConstraintUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceConstraintProjectionReducer.Apply(s, e), "ServiceConstraintUpdatedEvent", null, ct);

    public Task HandleAsync(ServiceConstraintActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceConstraintProjectionReducer.Apply(s, e), "ServiceConstraintActivatedEvent", null, ct);

    public Task HandleAsync(ServiceConstraintArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ServiceConstraintProjectionReducer.Apply(s, e), "ServiceConstraintArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ServiceConstraintReadModel, ServiceConstraintReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ServiceConstraintReadModel { ServiceConstraintId = aggregateId };
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
