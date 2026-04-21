using Whycespace.Projections.Business.Customer.IdentityAndProfile.Customer.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Customer.IdentityAndProfile.Customer;

public sealed class CustomerProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<CustomerCreatedEventSchema>,
    IProjectionHandler<CustomerRenamedEventSchema>,
    IProjectionHandler<CustomerReclassifiedEventSchema>,
    IProjectionHandler<CustomerActivatedEventSchema>,
    IProjectionHandler<CustomerArchivedEventSchema>
{
    private readonly PostgresProjectionStore<CustomerReadModel> _store;

    public CustomerProjectionHandler(PostgresProjectionStore<CustomerReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            CustomerCreatedEventSchema e      => Project(e.AggregateId, s => CustomerProjectionReducer.Apply(s, e), "CustomerCreatedEvent",      envelope, cancellationToken),
            CustomerRenamedEventSchema e      => Project(e.AggregateId, s => CustomerProjectionReducer.Apply(s, e), "CustomerRenamedEvent",      envelope, cancellationToken),
            CustomerReclassifiedEventSchema e => Project(e.AggregateId, s => CustomerProjectionReducer.Apply(s, e), "CustomerReclassifiedEvent", envelope, cancellationToken),
            CustomerActivatedEventSchema e    => Project(e.AggregateId, s => CustomerProjectionReducer.Apply(s, e), "CustomerActivatedEvent",    envelope, cancellationToken),
            CustomerArchivedEventSchema e     => Project(e.AggregateId, s => CustomerProjectionReducer.Apply(s, e), "CustomerArchivedEvent",     envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CustomerProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(CustomerCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CustomerProjectionReducer.Apply(s, e), "CustomerCreatedEvent", null, ct);

    public Task HandleAsync(CustomerRenamedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CustomerProjectionReducer.Apply(s, e), "CustomerRenamedEvent", null, ct);

    public Task HandleAsync(CustomerReclassifiedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CustomerProjectionReducer.Apply(s, e), "CustomerReclassifiedEvent", null, ct);

    public Task HandleAsync(CustomerActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CustomerProjectionReducer.Apply(s, e), "CustomerActivatedEvent", null, ct);

    public Task HandleAsync(CustomerArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CustomerProjectionReducer.Apply(s, e), "CustomerArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CustomerReadModel, CustomerReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CustomerReadModel { CustomerId = aggregateId };
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
