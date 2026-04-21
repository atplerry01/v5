using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Stewardship.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Stewardship;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Stewardship;

namespace Whycespace.Projections.Structural.Humancapital.Stewardship;

public sealed class StewardshipProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<StewardshipCreatedEventSchema>
{
    private readonly PostgresProjectionStore<StewardshipReadModel> _store;

    public StewardshipProjectionHandler(PostgresProjectionStore<StewardshipReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            StewardshipCreatedEventSchema e => Project(e.AggregateId, s => StewardshipProjectionReducer.Apply(s, e, envelope.Timestamp), "StewardshipCreatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"StewardshipProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(StewardshipCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StewardshipProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "StewardshipCreatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<StewardshipReadModel, StewardshipReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new StewardshipReadModel { StewardshipId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
