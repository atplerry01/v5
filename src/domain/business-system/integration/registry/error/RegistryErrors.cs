namespace Whycespace.Domain.BusinessSystem.Integration.Registry;

public static class RegistryErrors
{
    public static RegistryDomainException MissingId()
        => new("RegistryId is required and must not be empty.");

    public static RegistryDomainException MissingEntryId()
        => new("RegistryEntryId is required and must not be empty.");

    public static RegistryDomainException AlreadyActive(RegistryId id)
        => new($"Registry '{id.Value}' is already active.");

    public static RegistryDomainException AlreadyDeactivated(RegistryId id)
        => new($"Registry '{id.Value}' is already deactivated.");

    public static RegistryDomainException InvalidStateTransition(RegistryStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class RegistryDomainException : Exception
{
    public RegistryDomainException(string message) : base(message) { }
}
