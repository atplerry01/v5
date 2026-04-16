using Whycespace.Projections.Economic.Risk.Exposure.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Risk.Exposure;

public sealed class RiskExposureProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<RiskExposureCreatedEventSchema>,
    IProjectionHandler<RiskExposureIncreasedEventSchema>,
    IProjectionHandler<RiskExposureReducedEventSchema>,
    IProjectionHandler<RiskExposureClosedEventSchema>
{
    private readonly PostgresProjectionStore<RiskExposureReadModel> _store;

    public RiskExposureProjectionHandler(PostgresProjectionStore<RiskExposureReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            RiskExposureCreatedEventSchema e   => Project(e.AggregateId, s => RiskExposureProjectionReducer.Apply(s, e), "RiskExposureCreatedEvent", envelope, cancellationToken),
            RiskExposureIncreasedEventSchema e => Project(e.AggregateId, s => RiskExposureProjectionReducer.Apply(s, e), "RiskExposureIncreasedEvent", envelope, cancellationToken),
            RiskExposureReducedEventSchema e   => Project(e.AggregateId, s => RiskExposureProjectionReducer.Apply(s, e), "RiskExposureReducedEvent", envelope, cancellationToken),
            RiskExposureClosedEventSchema e    => Project(e.AggregateId, s => RiskExposureProjectionReducer.Apply(s, e), "RiskExposureClosedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"RiskExposureProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(RiskExposureCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RiskExposureProjectionReducer.Apply(s, e), "RiskExposureCreatedEvent", null, ct);

    public Task HandleAsync(RiskExposureIncreasedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RiskExposureProjectionReducer.Apply(s, e), "RiskExposureIncreasedEvent", null, ct);

    public Task HandleAsync(RiskExposureReducedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RiskExposureProjectionReducer.Apply(s, e), "RiskExposureReducedEvent", null, ct);

    public Task HandleAsync(RiskExposureClosedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RiskExposureProjectionReducer.Apply(s, e), "RiskExposureClosedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<RiskExposureReadModel, RiskExposureReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new RiskExposureReadModel { ExposureId = aggregateId };
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
