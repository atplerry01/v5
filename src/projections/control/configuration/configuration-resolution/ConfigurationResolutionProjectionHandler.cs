using Whycespace.Projections.Control.Configuration.ConfigurationResolution.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationResolution;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationResolution;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Configuration.ConfigurationResolution;

public sealed class ConfigurationResolutionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ConfigurationResolvedEventSchema>
{
    private readonly PostgresProjectionStore<ConfigurationResolutionReadModel> _store;

    public ConfigurationResolutionProjectionHandler(PostgresProjectionStore<ConfigurationResolutionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ConfigurationResolvedEventSchema e => Project(e.AggregateId, s => ConfigurationResolutionProjectionReducer.Apply(s, e), "ConfigurationResolvedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ConfigurationResolutionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ConfigurationResolvedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationResolutionProjectionReducer.Apply(s, e), "ConfigurationResolvedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ConfigurationResolutionReadModel, ConfigurationResolutionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ConfigurationResolutionReadModel { ResolutionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName,
            envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
