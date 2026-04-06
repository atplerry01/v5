namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Strength of a relationship edge in the intelligence graph (0.0–1.0).
/// </summary>
public sealed record RelationshipStrength
{
    public decimal Value { get; }

    public RelationshipStrength(decimal value)
    {
        Value = Math.Clamp(value, 0m, 1m);
    }

    public static RelationshipStrength Weak => new(0.25m);
    public static RelationshipStrength Moderate => new(0.5m);
    public static RelationshipStrength Strong => new(0.75m);
    public static RelationshipStrength Absolute => new(1.0m);

    public override string ToString() => Value.ToString("F2");
}
