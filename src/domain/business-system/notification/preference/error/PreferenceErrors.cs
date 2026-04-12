namespace Whycespace.Domain.BusinessSystem.Notification.Preference;

public static class PreferenceErrors
{
    public static PreferenceDomainException MissingId()
        => new("PreferenceId is required and must not be empty.");

    public static PreferenceDomainException InvalidRule()
        => new("Preference must define a valid rule with owner reference.");

    public static PreferenceDomainException InvalidStateTransition(PreferenceStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class PreferenceDomainException : Exception
{
    public PreferenceDomainException(string message) : base(message) { }
}
