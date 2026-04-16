using Whycespace.Projections.Economic.Enforcement.Rule.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Enforcement.Rule;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Rule;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Enforcement.Rule;

public sealed class EnforcementRuleProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<EnforcementRuleDefinedEventSchema>,
    IProjectionHandler<EnforcementRuleActivatedEventSchema>,
    IProjectionHandler<EnforcementRuleDisabledEventSchema>,
    IProjectionHandler<EnforcementRuleRetiredEventSchema>
{
    private readonly PostgresProjectionStore<EnforcementRuleReadModel> _store;

    public EnforcementRuleProjectionHandler(PostgresProjectionStore<EnforcementRuleReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            EnforcementRuleDefinedEventSchema e    => Project(e.AggregateId, s => EnforcementRuleProjectionReducer.Apply(s, e), "EnforcementRuleDefinedEvent", envelope, cancellationToken),
            EnforcementRuleActivatedEventSchema e  => Project(e.AggregateId, s => EnforcementRuleProjectionReducer.Apply(s, e), "EnforcementRuleActivatedEvent", envelope, cancellationToken),
            EnforcementRuleDisabledEventSchema e   => Project(e.AggregateId, s => EnforcementRuleProjectionReducer.Apply(s, e), "EnforcementRuleDisabledEvent", envelope, cancellationToken),
            EnforcementRuleRetiredEventSchema e    => Project(e.AggregateId, s => EnforcementRuleProjectionReducer.Apply(s, e), "EnforcementRuleRetiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"EnforcementRuleProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(EnforcementRuleDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EnforcementRuleProjectionReducer.Apply(s, e), "EnforcementRuleDefinedEvent", null, ct);

    public Task HandleAsync(EnforcementRuleActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EnforcementRuleProjectionReducer.Apply(s, e), "EnforcementRuleActivatedEvent", null, ct);

    public Task HandleAsync(EnforcementRuleDisabledEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EnforcementRuleProjectionReducer.Apply(s, e), "EnforcementRuleDisabledEvent", null, ct);

    public Task HandleAsync(EnforcementRuleRetiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EnforcementRuleProjectionReducer.Apply(s, e), "EnforcementRuleRetiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<EnforcementRuleReadModel, EnforcementRuleReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new EnforcementRuleReadModel { RuleId = aggregateId };
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
