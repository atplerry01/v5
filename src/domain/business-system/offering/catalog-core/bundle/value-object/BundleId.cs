using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public readonly record struct BundleId
{
    public Guid Value { get; }

    public BundleId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "BundleId cannot be empty.");
        Value = value;
    }
}
