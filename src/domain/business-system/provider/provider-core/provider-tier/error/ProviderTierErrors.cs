using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public static class ProviderTierErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ProviderTierId is required and must not be empty.");

    public static DomainException InvalidStateTransition(ProviderTierStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ProviderTierId id)
        => new DomainInvariantViolationException($"ProviderTier '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ProviderTier has already been initialized.");
}
