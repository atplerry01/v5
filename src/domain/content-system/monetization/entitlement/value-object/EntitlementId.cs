namespace Whycespace.Domain.ContentSystem.Monetization.Entitlement;

public readonly record struct EntitlementId(Guid Value)
{
    public static EntitlementId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
