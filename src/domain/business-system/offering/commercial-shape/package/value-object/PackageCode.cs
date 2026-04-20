namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public readonly record struct PackageCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public PackageCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PackageCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"PackageCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
