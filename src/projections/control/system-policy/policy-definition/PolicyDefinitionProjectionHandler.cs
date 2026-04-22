using Whycespace.Projections.Control.SystemPolicy.PolicyDefinition.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDefinition;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyDefinition;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.SystemPolicy.PolicyDefinition;

public sealed class PolicyDefinitionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PolicyDefinedEventSchema>,
    IProjectionHandler<PolicyDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<PolicyDefinitionReadModel> _store;

    public PolicyDefinitionProjectionHandler(PostgresProjectionStore<PolicyDefinitionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PolicyDefinedEventSchema e    => Project(e.AggregateId, s => PolicyDefinitionProjectionReducer.Apply(s, e), "PolicyDefinedEvent",    envelope, cancellationToken),
            PolicyDeprecatedEventSchema e => Project(e.AggregateId, s => PolicyDefinitionProjectionReducer.Apply(s, e), "PolicyDeprecatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PolicyDefinitionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PolicyDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyDefinitionProjectionReducer.Apply(s, e), "PolicyDefinedEvent", null, ct);

    public Task HandleAsync(PolicyDeprecatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyDefinitionProjectionReducer.Apply(s, e), "PolicyDeprecatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<PolicyDefinitionReadModel, PolicyDefinitionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new PolicyDefinitionReadModel { PolicyId = aggregateId };
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
