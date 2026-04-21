using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public static class ProviderAgreementErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ProviderAgreementId is required and must not be empty.");

    public static DomainException MissingProviderRef()
        => new DomainInvariantViolationException("ProviderAgreement must reference a provider.");

    public static DomainException InvalidStateTransition(ProviderAgreementStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ActivationRequiresEffectiveWindow()
        => new DomainInvariantViolationException("ProviderAgreement requires an effective window before activation.");

    public static DomainException AlreadyTerminated(ProviderAgreementId id)
        => new DomainInvariantViolationException($"ProviderAgreement '{id.Value}' has already been terminated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ProviderAgreement has already been initialized.");
}
