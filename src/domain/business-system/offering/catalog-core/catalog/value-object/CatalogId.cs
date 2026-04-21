using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public readonly record struct CatalogId
{
    public Guid Value { get; }

    public CatalogId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CatalogId cannot be empty.");
        Value = value;
    }
}
