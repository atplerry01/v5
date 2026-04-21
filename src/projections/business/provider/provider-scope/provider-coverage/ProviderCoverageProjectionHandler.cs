using Whycespace.Projections.Business.Provider.ProviderScope.ProviderCoverage.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Provider.ProviderScope.ProviderCoverage;

public sealed class ProviderCoverageProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ProviderCoverageCreatedEventSchema>,
    IProjectionHandler<CoverageScopeAddedEventSchema>,
    IProjectionHandler<CoverageScopeRemovedEventSchema>,
    IProjectionHandler<ProviderCoverageActivatedEventSchema>,
    IProjectionHandler<ProviderCoverageArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ProviderCoverageReadModel> _store;

    public ProviderCoverageProjectionHandler(PostgresProjectionStore<ProviderCoverageReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ProviderCoverageCreatedEventSchema e   => Project(e.AggregateId, s => ProviderCoverageProjectionReducer.Apply(s, e), "ProviderCoverageCreatedEvent",   envelope, cancellationToken),
            CoverageScopeAddedEventSchema e        => Project(e.AggregateId, s => ProviderCoverageProjectionReducer.Apply(s, e), "CoverageScopeAddedEvent",        envelope, cancellationToken),
            CoverageScopeRemovedEventSchema e      => Project(e.AggregateId, s => ProviderCoverageProjectionReducer.Apply(s, e), "CoverageScopeRemovedEvent",      envelope, cancellationToken),
            ProviderCoverageActivatedEventSchema e => Project(e.AggregateId, s => ProviderCoverageProjectionReducer.Apply(s, e), "ProviderCoverageActivatedEvent", envelope, cancellationToken),
            ProviderCoverageArchivedEventSchema e  => Project(e.AggregateId, s => ProviderCoverageProjectionReducer.Apply(s, e), "ProviderCoverageArchivedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ProviderCoverageProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ProviderCoverageCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderCoverageProjectionReducer.Apply(s, e), "ProviderCoverageCreatedEvent", null, ct);

    public Task HandleAsync(CoverageScopeAddedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderCoverageProjectionReducer.Apply(s, e), "CoverageScopeAddedEvent", null, ct);

    public Task HandleAsync(CoverageScopeRemovedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderCoverageProjectionReducer.Apply(s, e), "CoverageScopeRemovedEvent", null, ct);

    public Task HandleAsync(ProviderCoverageActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderCoverageProjectionReducer.Apply(s, e), "ProviderCoverageActivatedEvent", null, ct);

    public Task HandleAsync(ProviderCoverageArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProviderCoverageProjectionReducer.Apply(s, e), "ProviderCoverageArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ProviderCoverageReadModel, ProviderCoverageReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ProviderCoverageReadModel { ProviderCoverageId = aggregateId };
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
