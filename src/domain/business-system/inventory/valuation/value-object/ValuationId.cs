namespace Whycespace.Domain.BusinessSystem.Inventory.Valuation;

public readonly record struct ValuationId
{
    public Guid Value { get; }

    public ValuationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ValuationId value must not be empty.", nameof(value));
        Value = value;
    }
}
