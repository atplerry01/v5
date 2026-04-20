namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(CapabilityStatus status) => status == CapabilityStatus.Draft;
}
