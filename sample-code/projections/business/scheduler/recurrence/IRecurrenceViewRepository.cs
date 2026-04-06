namespace Whycespace.Projections.Business.Scheduler.Recurrence;

public interface IRecurrenceViewRepository
{
    Task SaveAsync(RecurrenceReadModel model, CancellationToken ct = default);
    Task<RecurrenceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
