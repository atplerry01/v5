using Whycespace.Projections.Platform.Schema.Versioning.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Schema.Versioning;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Schema.Versioning;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Schema.Versioning;

public sealed class VersioningRuleProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<VersioningRuleRegisteredEventSchema>,
    IProjectionHandler<VersioningRuleVerdictIssuedEventSchema>
{
    private readonly PostgresProjectionStore<VersioningRuleReadModel> _store;

    public VersioningRuleProjectionHandler(PostgresProjectionStore<VersioningRuleReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            VersioningRuleRegisteredEventSchema e => Project(e.AggregateId, s => VersioningRuleProjectionReducer.Apply(s, e, envelope.Timestamp), "VersioningRuleRegisteredEvent", envelope, cancellationToken),
            VersioningRuleVerdictIssuedEventSchema e => Project(e.AggregateId, s => VersioningRuleProjectionReducer.Apply(s, e, envelope.Timestamp), "VersioningRuleVerdictIssuedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"VersioningRuleProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(VersioningRuleRegisteredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => VersioningRuleProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "VersioningRuleRegisteredEvent", null, ct);
    public Task HandleAsync(VersioningRuleVerdictIssuedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => VersioningRuleProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "VersioningRuleVerdictIssuedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<VersioningRuleReadModel, VersioningRuleReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new VersioningRuleReadModel { VersioningRuleId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
