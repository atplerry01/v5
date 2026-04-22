using Whycespace.Projections.Control.Configuration.ConfigurationAssignment.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationAssignment;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationAssignment;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Configuration.ConfigurationAssignment;

public sealed class ConfigurationAssignmentProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ConfigurationAssignedEventSchema>,
    IProjectionHandler<ConfigurationAssignmentRevokedEventSchema>
{
    private readonly PostgresProjectionStore<ConfigurationAssignmentReadModel> _store;

    public ConfigurationAssignmentProjectionHandler(PostgresProjectionStore<ConfigurationAssignmentReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ConfigurationAssignedEventSchema e         => Project(e.AggregateId, s => ConfigurationAssignmentProjectionReducer.Apply(s, e), "ConfigurationAssignedEvent",         envelope, cancellationToken),
            ConfigurationAssignmentRevokedEventSchema e => Project(e.AggregateId, s => ConfigurationAssignmentProjectionReducer.Apply(s, e), "ConfigurationAssignmentRevokedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ConfigurationAssignmentProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ConfigurationAssignedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationAssignmentProjectionReducer.Apply(s, e), "ConfigurationAssignedEvent", null, ct);

    public Task HandleAsync(ConfigurationAssignmentRevokedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConfigurationAssignmentProjectionReducer.Apply(s, e), "ConfigurationAssignmentRevokedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ConfigurationAssignmentReadModel, ConfigurationAssignmentReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ConfigurationAssignmentReadModel { AssignmentId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName,
            envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
