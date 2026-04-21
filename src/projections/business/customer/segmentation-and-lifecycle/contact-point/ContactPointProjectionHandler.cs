using Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.ContactPoint.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed class ContactPointProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ContactPointCreatedEventSchema>,
    IProjectionHandler<ContactPointUpdatedEventSchema>,
    IProjectionHandler<ContactPointActivatedEventSchema>,
    IProjectionHandler<ContactPointPreferredSetEventSchema>,
    IProjectionHandler<ContactPointArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ContactPointReadModel> _store;

    public ContactPointProjectionHandler(PostgresProjectionStore<ContactPointReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ContactPointCreatedEventSchema e       => Project(e.AggregateId, s => ContactPointProjectionReducer.Apply(s, e), "ContactPointCreatedEvent",       envelope, cancellationToken),
            ContactPointUpdatedEventSchema e       => Project(e.AggregateId, s => ContactPointProjectionReducer.Apply(s, e), "ContactPointUpdatedEvent",       envelope, cancellationToken),
            ContactPointActivatedEventSchema e     => Project(e.AggregateId, s => ContactPointProjectionReducer.Apply(s, e), "ContactPointActivatedEvent",     envelope, cancellationToken),
            ContactPointPreferredSetEventSchema e  => Project(e.AggregateId, s => ContactPointProjectionReducer.Apply(s, e), "ContactPointPreferredSetEvent",  envelope, cancellationToken),
            ContactPointArchivedEventSchema e      => Project(e.AggregateId, s => ContactPointProjectionReducer.Apply(s, e), "ContactPointArchivedEvent",      envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ContactPointProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ContactPointCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ContactPointProjectionReducer.Apply(s, e), "ContactPointCreatedEvent", null, ct);

    public Task HandleAsync(ContactPointUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ContactPointProjectionReducer.Apply(s, e), "ContactPointUpdatedEvent", null, ct);

    public Task HandleAsync(ContactPointActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ContactPointProjectionReducer.Apply(s, e), "ContactPointActivatedEvent", null, ct);

    public Task HandleAsync(ContactPointPreferredSetEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ContactPointProjectionReducer.Apply(s, e), "ContactPointPreferredSetEvent", null, ct);

    public Task HandleAsync(ContactPointArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ContactPointProjectionReducer.Apply(s, e), "ContactPointArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ContactPointReadModel, ContactPointReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ContactPointReadModel { ContactPointId = aggregateId };
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
