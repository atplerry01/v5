namespace Whycespace.Projections.Core.Temporal.ScheduleReference;

public interface IScheduleReferenceViewRepository
{
    Task SaveAsync(ScheduleReferenceReadModel model, CancellationToken ct = default);
    Task<ScheduleReferenceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
