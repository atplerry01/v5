namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ProviderCoverageStatus status) => status != ProviderCoverageStatus.Archived;
}
