namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ProviderCoverageStatus status, int scopeCount)
        => status == ProviderCoverageStatus.Draft && scopeCount > 0;
}
