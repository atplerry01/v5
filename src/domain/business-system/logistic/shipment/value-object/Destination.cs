namespace Whycespace.Domain.BusinessSystem.Logistic.Shipment;

public readonly record struct Destination
{
    public string Value { get; }

    public Destination(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Destination must not be empty.", nameof(value));

        Value = value;
    }
}
