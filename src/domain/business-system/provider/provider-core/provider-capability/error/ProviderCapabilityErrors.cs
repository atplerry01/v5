namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public static class ProviderCapabilityErrors
{
    public static ProviderCapabilityDomainException MissingId()
        => new("ProviderCapabilityId is required and must not be empty.");

    public static ProviderCapabilityDomainException MissingProviderRef()
        => new("ProviderCapability must reference a provider.");

    public static ProviderCapabilityDomainException InvalidStateTransition(CapabilityStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ProviderCapabilityDomainException ArchivedImmutable(ProviderCapabilityId id)
        => new($"ProviderCapability '{id.Value}' is archived and cannot be mutated.");
}

public sealed class ProviderCapabilityDomainException : Exception
{
    public ProviderCapabilityDomainException(string message) : base(message) { }
}
