namespace Whycespace.Domain.BusinessSystem.Scheduler.Slot;

public readonly record struct SlotId
{
    public Guid Value { get; }

    public SlotId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SlotId value must not be empty.", nameof(value));
        Value = value;
    }
}
