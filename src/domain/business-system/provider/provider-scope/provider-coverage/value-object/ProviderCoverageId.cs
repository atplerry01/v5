using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public readonly record struct ProviderCoverageId
{
    public Guid Value { get; }

    public ProviderCoverageId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProviderCoverageId cannot be empty.");
        Value = value;
    }
}
