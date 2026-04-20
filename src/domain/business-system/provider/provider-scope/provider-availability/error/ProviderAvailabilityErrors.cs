namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public static class ProviderAvailabilityErrors
{
    public static ProviderAvailabilityDomainException MissingId()
        => new("ProviderAvailabilityId is required and must not be empty.");

    public static ProviderAvailabilityDomainException MissingProviderRef()
        => new("ProviderAvailability must reference a provider.");

    public static ProviderAvailabilityDomainException InvalidStateTransition(ProviderAvailabilityStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ProviderAvailabilityDomainException ArchivedImmutable(ProviderAvailabilityId id)
        => new($"ProviderAvailability '{id.Value}' is archived and cannot be mutated.");
}

public sealed class ProviderAvailabilityDomainException : Exception
{
    public ProviderAvailabilityDomainException(string message) : base(message) { }
}
