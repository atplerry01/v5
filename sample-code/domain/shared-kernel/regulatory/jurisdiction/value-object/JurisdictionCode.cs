namespace Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

/// <summary>
/// ISO 3166-1 alpha-2 country code (e.g. "US", "GB", "DE").
/// </summary>
public sealed record JurisdictionCode
{
    public string Value { get; }

    public JurisdictionCode(string value)
    {
        Guard.AgainstEmpty(value, nameof(value));
        Guard.AgainstInvalid(value, v => v.Length == 2 && v == v.ToUpperInvariant(),
            "Jurisdiction code must be a 2-letter uppercase ISO 3166-1 alpha-2 code.", nameof(value));

        Value = value;
    }
}