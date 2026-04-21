using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public readonly record struct CatalogRef
{
    public Guid Value { get; }

    public CatalogRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CatalogRef cannot be empty.");
        Value = value;
    }
}
