namespace Whycespace.Domain.BusinessSystem.Localization.Locale;

public static class LocaleErrors
{
    public static LocaleDomainException MissingId()
        => new("LocaleId is required and must not be empty.");

    public static LocaleDomainException InvalidLocaleCode()
        => new("Locale must define both language and region.");

    public static LocaleDomainException InvalidStateTransition(LocaleStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static LocaleDomainException DuplicateLocale(LocaleCode code)
        => new($"Locale '{code.Language}-{code.Region}' already exists.");
}

public sealed class LocaleDomainException : Exception
{
    public LocaleDomainException(string message) : base(message) { }
}
