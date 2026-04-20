namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public static class ConfigurationErrors
{
    public static ConfigurationDomainException MissingId()
        => new("ConfigurationId is required and must not be empty.");

    public static ConfigurationDomainException InvalidStateTransition(ConfigurationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ConfigurationDomainException ArchivedImmutable(ConfigurationId id)
        => new($"Configuration '{id.Value}' is archived and cannot be mutated.");

    public static ConfigurationDomainException OptionNotPresent(string key)
        => new($"Configuration does not contain option '{key}'.");
}

public sealed class ConfigurationDomainException : Exception
{
    public ConfigurationDomainException(string message) : base(message) { }
}
