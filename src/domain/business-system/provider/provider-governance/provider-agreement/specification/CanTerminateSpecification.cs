namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public sealed class CanTerminateSpecification
{
    public bool IsSatisfiedBy(ProviderAgreementStatus status) => status != ProviderAgreementStatus.Terminated;
}
