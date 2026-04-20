namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public static class ProviderAgreementErrors
{
    public static ProviderAgreementDomainException MissingId()
        => new("ProviderAgreementId is required and must not be empty.");

    public static ProviderAgreementDomainException MissingProviderRef()
        => new("ProviderAgreement must reference a provider.");

    public static ProviderAgreementDomainException InvalidStateTransition(ProviderAgreementStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ProviderAgreementDomainException ActivationRequiresEffectiveWindow()
        => new("ProviderAgreement requires an effective window before activation.");

    public static ProviderAgreementDomainException AlreadyTerminated(ProviderAgreementId id)
        => new($"ProviderAgreement '{id.Value}' has already been terminated.");
}

public sealed class ProviderAgreementDomainException : Exception
{
    public ProviderAgreementDomainException(string message) : base(message) { }
}
