namespace Whycespace.Domain.BusinessSystem.Notification.Preference;

public readonly record struct PreferenceId
{
    public Guid Value { get; }

    public PreferenceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PreferenceId value must not be empty.", nameof(value));

        Value = value;
    }
}