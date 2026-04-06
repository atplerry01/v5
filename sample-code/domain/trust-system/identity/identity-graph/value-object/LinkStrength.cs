namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed record LinkStrength(string Value)
{
    public static readonly LinkStrength Strong = new("Strong");
    public static readonly LinkStrength Medium = new("Medium");
    public static readonly LinkStrength Weak = new("Weak");

    public override string ToString() => Value;
}
