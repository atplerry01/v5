namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(ProviderAgreementStatus status) => status == ProviderAgreementStatus.Active;
}
