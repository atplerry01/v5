namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ProviderAgreementStatus status) => status is ProviderAgreementStatus.Draft or ProviderAgreementStatus.Suspended;
}
