using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public readonly record struct BundleName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public BundleName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "BundleName must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"BundleName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
