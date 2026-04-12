namespace Whycespace.Domain.BusinessSystem.Scheduler.Calendar;

public readonly record struct CalendarId
{
    public Guid Value { get; }

    public CalendarId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CalendarId value must not be empty.", nameof(value));
        Value = value;
    }
}
