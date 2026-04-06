namespace Whycespace.Projections.Business.Scheduler.Schedule;

public interface IScheduleViewRepository
{
    Task SaveAsync(ScheduleReadModel model, CancellationToken ct = default);
    Task<ScheduleReadModel?> GetAsync(string id, CancellationToken ct = default);
}
