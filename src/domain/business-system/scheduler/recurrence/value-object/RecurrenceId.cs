namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public readonly record struct RecurrenceId
{
    public Guid Value { get; }

    public RecurrenceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RecurrenceId value must not be empty.", nameof(value));
        Value = value;
    }
}
