using Whycespace.Projections.Economic.Revenue.Revenue.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Revenue.Revenue;

public sealed class RevenueRecordedProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<RevenueRecordedEventSchema>
{
    private readonly PostgresProjectionStore<RevenueReadModel> _store;

    public RevenueRecordedProjectionHandler(PostgresProjectionStore<RevenueReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            RevenueRecordedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"RevenueRecordedProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(RevenueRecordedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(RevenueRecordedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new RevenueReadModel { RevenueId = e.AggregateId };
        state = RevenueProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "RevenueRecordedEvent", eventId, eventVersion, correlationId, ct);
    }
}
