namespace Whycespace.Projections.Core.Temporal.Timeline;

public interface ITimelineViewRepository
{
    Task SaveAsync(TimelineReadModel model, CancellationToken ct = default);
    Task<TimelineReadModel?> GetAsync(string id, CancellationToken ct = default);
}
