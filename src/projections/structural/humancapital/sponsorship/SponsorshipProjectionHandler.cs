using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Sponsorship.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Sponsorship;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Sponsorship;

namespace Whycespace.Projections.Structural.Humancapital.Sponsorship;

public sealed class SponsorshipProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SponsorshipCreatedEventSchema>
{
    private readonly PostgresProjectionStore<SponsorshipReadModel> _store;

    public SponsorshipProjectionHandler(PostgresProjectionStore<SponsorshipReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            SponsorshipCreatedEventSchema e => Project(e.AggregateId, s => SponsorshipProjectionReducer.Apply(s, e, envelope.Timestamp), "SponsorshipCreatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SponsorshipProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(SponsorshipCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SponsorshipProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SponsorshipCreatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<SponsorshipReadModel, SponsorshipReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new SponsorshipReadModel { SponsorshipId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
