using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public readonly record struct ProductName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ProductName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ProductName must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"ProductName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
