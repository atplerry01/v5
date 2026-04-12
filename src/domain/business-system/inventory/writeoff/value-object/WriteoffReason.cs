namespace Whycespace.Domain.BusinessSystem.Inventory.Writeoff;

public readonly record struct WriteoffReason
{
    public string Value { get; }

    public WriteoffReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("WriteoffReason must not be empty.", nameof(value));
        Value = value;
    }
}
