using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public readonly record struct PackageName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public PackageName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "PackageName must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"PackageName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
