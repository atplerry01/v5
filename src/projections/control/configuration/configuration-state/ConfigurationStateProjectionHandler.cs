using Whycespace.Projections.Control.Configuration.ConfigurationState.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationState;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationState;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Configuration.ConfigurationState;

public sealed class ConfigurationStateProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ConfigurationStateSetEventSchema>,
    IProjectionHandler<ConfigurationStateRevokedEventSchema>
{
    private readonly PostgresProjectionStore<ConfigurationStateReadModel> _store;

    public ConfigurationStateProjectionHandler(PostgresProjectionStore<ConfigurationStateReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ConfigurationStateSetEventSchema e     => Project(e.AggregateId, s => ConfigurationStateProjectionReducer.Apply(s, e), "ConfigurationStateSetEvent",     envelope, cancellationToken),
            ConfigurationStateRevokedEventSchema e => Project(e.AggregateId, s => ConfigurationStateProjectionReducer.Apply(s, e), "ConfigurationStateRevokedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ConfigurationStateProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ConfigurationStateSetEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationStateProjectionReducer.Apply(s, e), "ConfigurationStateSetEvent", null, ct);

    public Task HandleAsync(ConfigurationStateRevokedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationStateProjectionReducer.Apply(s, e), "ConfigurationStateRevokedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ConfigurationStateReadModel, ConfigurationStateReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ConfigurationStateReadModel { StateId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName,
            envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
