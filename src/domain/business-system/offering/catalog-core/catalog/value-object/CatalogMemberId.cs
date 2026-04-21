using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public readonly record struct CatalogMemberId
{
    public Guid Value { get; }

    public CatalogMemberId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CatalogMemberId cannot be empty.");
        Value = value;
    }
}
