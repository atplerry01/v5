namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed record IdentityGraphStatus(string Value)
{
    public static readonly IdentityGraphStatus Active = new("Active");
    public static readonly IdentityGraphStatus Closed = new("Closed");

    public override string ToString() => Value;
}
