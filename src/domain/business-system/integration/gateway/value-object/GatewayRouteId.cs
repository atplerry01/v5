namespace Whycespace.Domain.BusinessSystem.Integration.Gateway;

public readonly record struct GatewayRouteId
{
    public Guid Value { get; }

    public GatewayRouteId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GatewayRouteId value must not be empty.", nameof(value));
        Value = value;
    }
}
