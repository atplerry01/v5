using Whycespace.Projections.Economic.Exchange.Rate.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Exchange.Rate;

public sealed class ExchangeRateProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ExchangeRateDefinedEventSchema>,
    IProjectionHandler<ExchangeRateActivatedEventSchema>,
    IProjectionHandler<ExchangeRateExpiredEventSchema>
{
    private readonly PostgresProjectionStore<ExchangeRateReadModel> _store;

    public ExchangeRateProjectionHandler(PostgresProjectionStore<ExchangeRateReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ExchangeRateDefinedEventSchema e   => Project(e.AggregateId, s => ExchangeRateProjectionReducer.Apply(s, e), "ExchangeRateDefinedEvent",   envelope, cancellationToken),
            ExchangeRateActivatedEventSchema e => Project(e.AggregateId, s => ExchangeRateProjectionReducer.Apply(s, e), "ExchangeRateActivatedEvent", envelope, cancellationToken),
            ExchangeRateExpiredEventSchema e   => Project(e.AggregateId, s => ExchangeRateProjectionReducer.Apply(s, e), "ExchangeRateExpiredEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ExchangeRateProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ExchangeRateDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ExchangeRateProjectionReducer.Apply(s, e), "ExchangeRateDefinedEvent", null, ct);

    public Task HandleAsync(ExchangeRateActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ExchangeRateProjectionReducer.Apply(s, e), "ExchangeRateActivatedEvent", null, ct);

    public Task HandleAsync(ExchangeRateExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ExchangeRateProjectionReducer.Apply(s, e), "ExchangeRateExpiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ExchangeRateReadModel, ExchangeRateReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ExchangeRateReadModel { RateId = aggregateId };
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
