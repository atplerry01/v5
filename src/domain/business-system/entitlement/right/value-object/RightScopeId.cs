namespace Whycespace.Domain.BusinessSystem.Entitlement.Right;

public readonly record struct RightScopeId
{
    public Guid Value { get; }

    public RightScopeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RightScopeId value must not be empty.", nameof(value));
        Value = value;
    }
}
