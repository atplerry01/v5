using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public static class ProviderAvailabilityErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ProviderAvailabilityId is required and must not be empty.");

    public static DomainException MissingProviderRef()
        => new DomainInvariantViolationException("ProviderAvailability must reference a provider.");

    public static DomainException InvalidStateTransition(ProviderAvailabilityStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ProviderAvailabilityId id)
        => new DomainInvariantViolationException($"ProviderAvailability '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ProviderAvailability has already been initialized.");
}
