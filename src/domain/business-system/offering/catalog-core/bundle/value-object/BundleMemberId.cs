using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public readonly record struct BundleMemberId
{
    public Guid Value { get; }

    public BundleMemberId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "BundleMemberId cannot be empty.");
        Value = value;
    }
}
