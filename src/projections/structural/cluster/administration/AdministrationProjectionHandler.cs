using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Cluster.Administration.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Cluster.Administration;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Cluster.Administration;

namespace Whycespace.Projections.Structural.Cluster.Administration;

public sealed class AdministrationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AdministrationEstablishedEventSchema>,
    IProjectionHandler<AdministrationAttachedEventSchema>,
    IProjectionHandler<AdministrationBindingValidatedEventSchema>,
    IProjectionHandler<AdministrationActivatedEventSchema>,
    IProjectionHandler<AdministrationSuspendedEventSchema>,
    IProjectionHandler<AdministrationRetiredEventSchema>
{
    private readonly PostgresProjectionStore<AdministrationReadModel> _store;

    public AdministrationProjectionHandler(PostgresProjectionStore<AdministrationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            AdministrationEstablishedEventSchema e => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, envelope.Timestamp), "AdministrationEstablishedEvent", envelope, cancellationToken),
            AdministrationAttachedEventSchema e => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, envelope.Timestamp), "AdministrationAttachedEvent", envelope, cancellationToken),
            AdministrationBindingValidatedEventSchema e => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, envelope.Timestamp), "AdministrationBindingValidatedEvent", envelope, cancellationToken),
            AdministrationActivatedEventSchema e => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, envelope.Timestamp), "AdministrationActivatedEvent", envelope, cancellationToken),
            AdministrationSuspendedEventSchema e => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, envelope.Timestamp), "AdministrationSuspendedEvent", envelope, cancellationToken),
            AdministrationRetiredEventSchema e => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, envelope.Timestamp), "AdministrationRetiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AdministrationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(AdministrationEstablishedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AdministrationEstablishedEvent", null, ct);
    public Task HandleAsync(AdministrationAttachedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AdministrationAttachedEvent", null, ct);
    public Task HandleAsync(AdministrationBindingValidatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AdministrationBindingValidatedEvent", null, ct);
    public Task HandleAsync(AdministrationActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AdministrationActivatedEvent", null, ct);
    public Task HandleAsync(AdministrationSuspendedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AdministrationSuspendedEvent", null, ct);
    public Task HandleAsync(AdministrationRetiredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AdministrationProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "AdministrationRetiredEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<AdministrationReadModel, AdministrationReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new AdministrationReadModel { AdministrationId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
