using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Structure.Classification.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Structure.Classification;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Structure.Classification;

namespace Whycespace.Projections.Structural.Structure.Classification;

public sealed class ClassificationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ClassificationDefinedEventSchema>,
    IProjectionHandler<ClassificationActivatedEventSchema>,
    IProjectionHandler<ClassificationDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<ClassificationReadModel> _store;

    public ClassificationProjectionHandler(PostgresProjectionStore<ClassificationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ClassificationDefinedEventSchema e => Project(e.AggregateId, s => ClassificationProjectionReducer.Apply(s, e, envelope.Timestamp), "ClassificationDefinedEvent", envelope, cancellationToken),
            ClassificationActivatedEventSchema e => Project(e.AggregateId, s => ClassificationProjectionReducer.Apply(s, e, envelope.Timestamp), "ClassificationActivatedEvent", envelope, cancellationToken),
            ClassificationDeprecatedEventSchema e => Project(e.AggregateId, s => ClassificationProjectionReducer.Apply(s, e, envelope.Timestamp), "ClassificationDeprecatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ClassificationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ClassificationDefinedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ClassificationProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ClassificationDefinedEvent", null, ct);
    public Task HandleAsync(ClassificationActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ClassificationProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ClassificationActivatedEvent", null, ct);
    public Task HandleAsync(ClassificationDeprecatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ClassificationProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ClassificationDeprecatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ClassificationReadModel, ClassificationReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ClassificationReadModel { ClassificationId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
