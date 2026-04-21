using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public readonly record struct PackageCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public PackageCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "PackageCode must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"PackageCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
