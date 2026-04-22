using Whycespace.Projections.Control.Configuration.ConfigurationScope.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationScope;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationScope;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Configuration.ConfigurationScope;

public sealed class ConfigurationScopeProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ConfigurationScopeDeclaredEventSchema>,
    IProjectionHandler<ConfigurationScopeRemovedEventSchema>
{
    private readonly PostgresProjectionStore<ConfigurationScopeReadModel> _store;

    public ConfigurationScopeProjectionHandler(PostgresProjectionStore<ConfigurationScopeReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ConfigurationScopeDeclaredEventSchema e => Project(e.AggregateId, s => ConfigurationScopeProjectionReducer.Apply(s, e), "ConfigurationScopeDeclaredEvent", envelope, cancellationToken),
            ConfigurationScopeRemovedEventSchema e  => Project(e.AggregateId, s => ConfigurationScopeProjectionReducer.Apply(s, e), "ConfigurationScopeRemovedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ConfigurationScopeProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ConfigurationScopeDeclaredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationScopeProjectionReducer.Apply(s, e), "ConfigurationScopeDeclaredEvent", null, ct);

    public Task HandleAsync(ConfigurationScopeRemovedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationScopeProjectionReducer.Apply(s, e), "ConfigurationScopeRemovedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ConfigurationScopeReadModel, ConfigurationScopeReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ConfigurationScopeReadModel { ScopeId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName,
            envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
