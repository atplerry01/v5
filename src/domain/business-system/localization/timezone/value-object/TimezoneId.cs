namespace Whycespace.Domain.BusinessSystem.Localization.Timezone;

public readonly record struct TimezoneId
{
    public Guid Value { get; }

    public TimezoneId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TimezoneId value must not be empty.", nameof(value));

        Value = value;
    }
}
