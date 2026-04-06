namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityCapability : IEquatable<EntityCapability>
{
    public static readonly EntityCapability CapitalPooling = new("CAPITAL_POOLING");
    public static readonly EntityCapability AssetHolding = new("ASSET_HOLDING");
    public static readonly EntityCapability Execution = new("EXECUTION");
    public static readonly EntityCapability ServiceProvision = new("SERVICE_PROVISION");
    public static readonly EntityCapability CustomerInterface = new("CUSTOMER_INTERFACE");
    public static readonly EntityCapability RevenueInterface = new("REVENUE_INTERFACE");

    public string Value { get; }

    private EntityCapability(string value)
    {
        Value = value;
    }

    public static EntityCapability From(string value)
    {
        return value.ToUpperInvariant() switch
        {
            "CAPITAL_POOLING" => CapitalPooling,
            "ASSET_HOLDING" => AssetHolding,
            "EXECUTION" => Execution,
            "SERVICE_PROVISION" => ServiceProvision,
            "CUSTOMER_INTERFACE" => CustomerInterface,
            "REVENUE_INTERFACE" => RevenueInterface,
            _ => throw new ArgumentException($"Invalid Capability: {value}")
        };
    }

    public bool Equals(EntityCapability? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is EntityCapability other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}
