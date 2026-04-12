namespace Whycespace.Domain.BusinessSystem.Integration.Gateway;

public readonly record struct GatewayId
{
    public Guid Value { get; }

    public GatewayId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GatewayId value must not be empty.", nameof(value));
        Value = value;
    }
}
