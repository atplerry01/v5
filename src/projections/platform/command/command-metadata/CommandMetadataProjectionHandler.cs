using Whycespace.Projections.Platform.Command.CommandMetadata.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Command.CommandMetadata;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Command.CommandMetadata;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Command.CommandMetadata;

public sealed class CommandMetadataProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<CommandMetadataAttachedEventSchema>
{
    private readonly PostgresProjectionStore<CommandMetadataReadModel> _store;

    public CommandMetadataProjectionHandler(PostgresProjectionStore<CommandMetadataReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            CommandMetadataAttachedEventSchema e => Project(e.AggregateId, s => CommandMetadataProjectionReducer.Apply(s, e, envelope.Timestamp), "CommandMetadataAttachedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"CommandMetadataProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(CommandMetadataAttachedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => CommandMetadataProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "CommandMetadataAttachedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<CommandMetadataReadModel, CommandMetadataReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new CommandMetadataReadModel { CommandMetadataId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
