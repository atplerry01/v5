using Whycespace.Projections.Platform.Command.CommandDefinition.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Command.CommandDefinition;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Command.CommandDefinition;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Command.CommandDefinition;

public sealed class CommandDefinitionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<CommandDefinedEventSchema>,
    IProjectionHandler<CommandDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<CommandDefinitionReadModel> _store;

    public CommandDefinitionProjectionHandler(PostgresProjectionStore<CommandDefinitionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            CommandDefinedEventSchema e => Project(e.AggregateId, s => CommandDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "CommandDefinedEvent", envelope, cancellationToken),
            CommandDeprecatedEventSchema e => Project(e.AggregateId, s => CommandDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "CommandDeprecatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"CommandDefinitionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(CommandDefinedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => CommandDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "CommandDefinedEvent", null, ct);
    public Task HandleAsync(CommandDeprecatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => CommandDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "CommandDeprecatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<CommandDefinitionReadModel, CommandDefinitionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new CommandDefinitionReadModel { CommandDefinitionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
