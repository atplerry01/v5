using Whycespace.Projections.Platform.Schema.Contract.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Schema.Contract;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Schema.Contract;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Schema.Contract;

public sealed class ContractProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ContractRegisteredEventSchema>,
    IProjectionHandler<ContractSubscriberAddedEventSchema>,
    IProjectionHandler<ContractDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<ContractReadModel> _store;

    public ContractProjectionHandler(PostgresProjectionStore<ContractReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ContractRegisteredEventSchema e => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e, envelope.Timestamp), "ContractRegisteredEvent", envelope, cancellationToken),
            ContractSubscriberAddedEventSchema e => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e, envelope.Timestamp), "ContractSubscriberAddedEvent", envelope, cancellationToken),
            ContractDeprecatedEventSchema e => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e, envelope.Timestamp), "ContractDeprecatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"ContractProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ContractRegisteredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ContractRegisteredEvent", null, ct);
    public Task HandleAsync(ContractSubscriberAddedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ContractSubscriberAddedEvent", null, ct);
    public Task HandleAsync(ContractDeprecatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ContractDeprecatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ContractReadModel, ContractReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ContractReadModel { ContractId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
