namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public readonly record struct ScheduleId
{
    public Guid Value { get; }

    public ScheduleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ScheduleId value must not be empty.", nameof(value));
        Value = value;
    }
}
