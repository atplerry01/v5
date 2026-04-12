namespace Whycespace.Domain.BusinessSystem.Entitlement.Right;

public readonly record struct RightId
{
    public Guid Value { get; }

    public RightId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RightId value must not be empty.", nameof(value));
        Value = value;
    }
}
