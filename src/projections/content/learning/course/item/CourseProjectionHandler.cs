using Whycespace.Projections.Content.Learning.Course.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Learning.Course;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Learning.Course;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Learning.Course.Item;

public sealed class CourseProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<CourseDraftedEventSchema>,
    IProjectionHandler<CourseModuleAttachedEventSchema>,
    IProjectionHandler<CourseModuleDetachedEventSchema>,
    IProjectionHandler<CoursePublishedEventSchema>,
    IProjectionHandler<CourseArchivedEventSchema>
{
    private readonly PostgresProjectionStore<CourseReadModel> _store;

    public CourseProjectionHandler(PostgresProjectionStore<CourseReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            CourseDraftedEventSchema e => Project(e.AggregateId, s => CourseProjectionReducer.Apply(s, e), "CourseDraftedEvent", envelope, cancellationToken),
            CourseModuleAttachedEventSchema e => Project(e.AggregateId, s => CourseProjectionReducer.Apply(s, e), "CourseModuleAttachedEvent", envelope, cancellationToken),
            CourseModuleDetachedEventSchema e => Project(e.AggregateId, s => CourseProjectionReducer.Apply(s, e), "CourseModuleDetachedEvent", envelope, cancellationToken),
            CoursePublishedEventSchema e => Project(e.AggregateId, s => CourseProjectionReducer.Apply(s, e), "CoursePublishedEvent", envelope, cancellationToken),
            CourseArchivedEventSchema e => Project(e.AggregateId, s => CourseProjectionReducer.Apply(s, e), "CourseArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CourseProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}. " +
                $"EventId={envelope.EventId}, EventType={envelope.EventType}.")
        };
    }

    public Task HandleAsync(CourseDraftedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => CourseProjectionReducer.Apply(s, e), "CourseDraftedEvent", null, ct);
    public Task HandleAsync(CourseModuleAttachedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => CourseProjectionReducer.Apply(s, e), "CourseModuleAttachedEvent", null, ct);
    public Task HandleAsync(CourseModuleDetachedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => CourseProjectionReducer.Apply(s, e), "CourseModuleDetachedEvent", null, ct);
    public Task HandleAsync(CoursePublishedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => CourseProjectionReducer.Apply(s, e), "CoursePublishedEvent", null, ct);
    public Task HandleAsync(CourseArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => CourseProjectionReducer.Apply(s, e), "CourseArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CourseReadModel, CourseReadModel> reduce,
        string eventType,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new CourseReadModel { Id = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventType,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
