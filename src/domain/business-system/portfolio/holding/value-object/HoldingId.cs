namespace Whycespace.Domain.BusinessSystem.Portfolio.Holding;

public readonly record struct HoldingId
{
    public Guid Value { get; }

    public HoldingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HoldingId value must not be empty.", nameof(value));

        Value = value;
    }
}
