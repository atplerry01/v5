namespace Whycespace.Domain.BusinessSystem.Scheduler.Availability;

public readonly record struct AvailabilityId
{
    public Guid Value { get; }

    public AvailabilityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AvailabilityId value must not be empty.", nameof(value));
        Value = value;
    }
}
