namespace Whycespace.Domain.BusinessSystem.Logistic.Shipment;

public readonly record struct Origin
{
    public string Value { get; }

    public Origin(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Origin must not be empty.", nameof(value));

        Value = value;
    }
}
