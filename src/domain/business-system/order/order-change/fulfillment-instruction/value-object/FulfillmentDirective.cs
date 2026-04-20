namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public readonly record struct FulfillmentDirective
{
    public const int MaxLength = 2000;

    public string Value { get; }

    public FulfillmentDirective(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("FulfillmentDirective must not be empty.", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"FulfillmentDirective exceeds {MaxLength} characters.", nameof(value));

        Value = value;
    }
}
