using Whycespace.Projections.Economic.Revenue.Pricing.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Revenue.Pricing;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Pricing;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Revenue.Pricing;

public sealed class PricingProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PriceDefinedEventSchema>,
    IProjectionHandler<PriceAdjustedEventSchema>
{
    private readonly PostgresProjectionStore<PricingReadModel> _store;

    public PricingProjectionHandler(PostgresProjectionStore<PricingReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PriceDefinedEventSchema e  => ProjectDefined(e, envelope, cancellationToken),
            PriceAdjustedEventSchema e => ProjectAdjusted(e, envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PricingProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(PriceDefinedEventSchema e, CancellationToken ct = default)
        => await ProjectDefined(e, null, ct);

    public async Task HandleAsync(PriceAdjustedEventSchema e, CancellationToken ct = default)
        => await ProjectAdjusted(e, null, ct);

    private async Task ProjectDefined(PriceDefinedEventSchema e, IEventEnvelope? env, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new PricingReadModel { PricingId = e.AggregateId };
        state = PricingProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "PriceDefinedEvent",
            env?.EventId ?? Guid.Empty, env?.SequenceNumber ?? 0, env?.CorrelationId ?? Guid.Empty, ct);
    }

    private async Task ProjectAdjusted(PriceAdjustedEventSchema e, IEventEnvelope? env, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new PricingReadModel { PricingId = e.AggregateId };
        state = PricingProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "PriceAdjustedEvent",
            env?.EventId ?? Guid.Empty, env?.SequenceNumber ?? 0, env?.CorrelationId ?? Guid.Empty, ct);
    }
}
