namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public readonly record struct ServiceOfferingName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ServiceOfferingName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ServiceOfferingName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ServiceOfferingName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
