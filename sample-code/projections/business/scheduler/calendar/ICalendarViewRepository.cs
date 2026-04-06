namespace Whycespace.Projections.Business.Scheduler.Calendar;

public interface ICalendarViewRepository
{
    Task SaveAsync(CalendarReadModel model, CancellationToken ct = default);
    Task<CalendarReadModel?> GetAsync(string id, CancellationToken ct = default);
}
