namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public readonly record struct PackageName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public PackageName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PackageName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"PackageName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
