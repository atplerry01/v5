using Whycespace.Projections.Business.Offering.CommercialShape.Package.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Offering.CommercialShape.Package;

public sealed class PackageProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PackageCreatedEventSchema>,
    IProjectionHandler<PackageMemberAddedEventSchema>,
    IProjectionHandler<PackageMemberRemovedEventSchema>,
    IProjectionHandler<PackageActivatedEventSchema>,
    IProjectionHandler<PackageArchivedEventSchema>
{
    private readonly PostgresProjectionStore<PackageReadModel> _store;

    public PackageProjectionHandler(PostgresProjectionStore<PackageReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PackageCreatedEventSchema e        => Project(e.AggregateId, s => PackageProjectionReducer.Apply(s, e), "PackageCreatedEvent",        envelope, cancellationToken),
            PackageMemberAddedEventSchema e    => Project(e.AggregateId, s => PackageProjectionReducer.Apply(s, e), "PackageMemberAddedEvent",    envelope, cancellationToken),
            PackageMemberRemovedEventSchema e  => Project(e.AggregateId, s => PackageProjectionReducer.Apply(s, e), "PackageMemberRemovedEvent",  envelope, cancellationToken),
            PackageActivatedEventSchema e      => Project(e.AggregateId, s => PackageProjectionReducer.Apply(s, e), "PackageActivatedEvent",      envelope, cancellationToken),
            PackageArchivedEventSchema e       => Project(e.AggregateId, s => PackageProjectionReducer.Apply(s, e), "PackageArchivedEvent",       envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PackageProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PackageCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PackageProjectionReducer.Apply(s, e), "PackageCreatedEvent", null, ct);

    public Task HandleAsync(PackageMemberAddedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PackageProjectionReducer.Apply(s, e), "PackageMemberAddedEvent", null, ct);

    public Task HandleAsync(PackageMemberRemovedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PackageProjectionReducer.Apply(s, e), "PackageMemberRemovedEvent", null, ct);

    public Task HandleAsync(PackageActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PackageProjectionReducer.Apply(s, e), "PackageActivatedEvent", null, ct);

    public Task HandleAsync(PackageArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PackageProjectionReducer.Apply(s, e), "PackageArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<PackageReadModel, PackageReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new PackageReadModel { PackageId = aggregateId };
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
