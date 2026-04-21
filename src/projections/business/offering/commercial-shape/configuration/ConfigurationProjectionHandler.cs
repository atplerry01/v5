using Whycespace.Projections.Business.Offering.CommercialShape.Configuration.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Offering.CommercialShape.Configuration;

public sealed class ConfigurationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ConfigurationCreatedEventSchema>,
    IProjectionHandler<ConfigurationOptionSetEventSchema>,
    IProjectionHandler<ConfigurationOptionRemovedEventSchema>,
    IProjectionHandler<ConfigurationActivatedEventSchema>,
    IProjectionHandler<ConfigurationArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ConfigurationReadModel> _store;

    public ConfigurationProjectionHandler(PostgresProjectionStore<ConfigurationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ConfigurationCreatedEventSchema e       => Project(e.AggregateId, s => ConfigurationProjectionReducer.Apply(s, e), "ConfigurationCreatedEvent",       envelope, cancellationToken),
            ConfigurationOptionSetEventSchema e     => Project(e.AggregateId, s => ConfigurationProjectionReducer.Apply(s, e), "ConfigurationOptionSetEvent",     envelope, cancellationToken),
            ConfigurationOptionRemovedEventSchema e => Project(e.AggregateId, s => ConfigurationProjectionReducer.Apply(s, e), "ConfigurationOptionRemovedEvent", envelope, cancellationToken),
            ConfigurationActivatedEventSchema e     => Project(e.AggregateId, s => ConfigurationProjectionReducer.Apply(s, e), "ConfigurationActivatedEvent",     envelope, cancellationToken),
            ConfigurationArchivedEventSchema e      => Project(e.AggregateId, s => ConfigurationProjectionReducer.Apply(s, e), "ConfigurationArchivedEvent",      envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ConfigurationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ConfigurationCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationProjectionReducer.Apply(s, e), "ConfigurationCreatedEvent", null, ct);

    public Task HandleAsync(ConfigurationOptionSetEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationProjectionReducer.Apply(s, e), "ConfigurationOptionSetEvent", null, ct);

    public Task HandleAsync(ConfigurationOptionRemovedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationProjectionReducer.Apply(s, e), "ConfigurationOptionRemovedEvent", null, ct);

    public Task HandleAsync(ConfigurationActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationProjectionReducer.Apply(s, e), "ConfigurationActivatedEvent", null, ct);

    public Task HandleAsync(ConfigurationArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationProjectionReducer.Apply(s, e), "ConfigurationArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ConfigurationReadModel, ConfigurationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ConfigurationReadModel { ConfigurationId = aggregateId };
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
