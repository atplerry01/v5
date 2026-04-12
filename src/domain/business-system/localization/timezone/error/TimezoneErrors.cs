namespace Whycespace.Domain.BusinessSystem.Localization.Timezone;

public static class TimezoneErrors
{
    public static TimezoneDomainException MissingId()
        => new("TimezoneId is required and must not be empty.");

    public static TimezoneDomainException InvalidTimezoneOffset()
        => new("Timezone must define a valid IANA identifier and UTC offset.");

    public static TimezoneDomainException InvalidStateTransition(TimezoneStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static TimezoneDomainException DuplicateTimezone(TimezoneOffset offset)
        => new($"Timezone '{offset.IanaId}' already exists.");
}

public sealed class TimezoneDomainException : Exception
{
    public TimezoneDomainException(string message) : base(message) { }
}
