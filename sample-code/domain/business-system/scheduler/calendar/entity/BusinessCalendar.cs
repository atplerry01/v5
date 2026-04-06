namespace Whycespace.Domain.BusinessSystem.Scheduler.Calendar;

public sealed class BusinessCalendar
{
    public CalendarId CalendarId { get; }
    public WorkingDays WorkingDays { get; }
    private readonly List<Holiday> _holidays;
    public IReadOnlyList<Holiday> Holidays => _holidays.AsReadOnly();

    public BusinessCalendar(CalendarId calendarId, WorkingDays workingDays, IEnumerable<Holiday> holidays)
    {
        CalendarId = calendarId;
        WorkingDays = workingDays;
        _holidays = holidays.ToList();
    }

    public bool IsBusinessDay(DateOnly date)
    {
        if (_holidays.Any(h => h.Date == date))
            return false;

        return WorkingDays.IsWorkingDay(date.DayOfWeek);
    }

    public DateOnly NextBusinessDay(DateOnly from)
    {
        var candidate = from.AddDays(1);
        while (!IsBusinessDay(candidate))
            candidate = candidate.AddDays(1);
        return candidate;
    }

    public void AddHoliday(Holiday holiday)
    {
        if (_holidays.All(h => h.Date != holiday.Date))
            _holidays.Add(holiday);
    }
}
