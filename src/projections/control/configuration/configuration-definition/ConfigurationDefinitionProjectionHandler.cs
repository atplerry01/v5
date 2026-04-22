using Whycespace.Projections.Control.Configuration.ConfigurationDefinition.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationDefinition;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationDefinition;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Configuration.ConfigurationDefinition;

public sealed class ConfigurationDefinitionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ConfigurationDefinedEventSchema>,
    IProjectionHandler<ConfigurationDefinitionDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<ConfigurationDefinitionReadModel> _store;

    public ConfigurationDefinitionProjectionHandler(PostgresProjectionStore<ConfigurationDefinitionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ConfigurationDefinedEventSchema e             => Project(e.AggregateId, s => ConfigurationDefinitionProjectionReducer.Apply(s, e), "ConfigurationDefinedEvent",             envelope, cancellationToken),
            ConfigurationDefinitionDeprecatedEventSchema e => Project(e.AggregateId, s => ConfigurationDefinitionProjectionReducer.Apply(s, e), "ConfigurationDefinitionDeprecatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ConfigurationDefinitionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ConfigurationDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationDefinitionProjectionReducer.Apply(s, e), "ConfigurationDefinedEvent", null, ct);

    public Task HandleAsync(ConfigurationDefinitionDeprecatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationDefinitionProjectionReducer.Apply(s, e), "ConfigurationDefinitionDeprecatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ConfigurationDefinitionReadModel, ConfigurationDefinitionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ConfigurationDefinitionReadModel { DefinitionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName,
            envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
