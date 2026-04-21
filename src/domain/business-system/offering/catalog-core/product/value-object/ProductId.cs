using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public readonly record struct ProductId
{
    public Guid Value { get; }

    public ProductId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProductId cannot be empty.");
        Value = value;
    }
}
