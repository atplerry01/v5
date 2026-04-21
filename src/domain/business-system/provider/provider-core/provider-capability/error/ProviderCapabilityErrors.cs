using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public static class ProviderCapabilityErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ProviderCapabilityId is required and must not be empty.");

    public static DomainException MissingProviderRef()
        => new DomainInvariantViolationException("ProviderCapability must reference a provider.");

    public static DomainException InvalidStateTransition(CapabilityStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ProviderCapabilityId id)
        => new DomainInvariantViolationException($"ProviderCapability '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ProviderCapability has already been initialized.");
}
