namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed record IdentityStatus(string Value)
{
    public static readonly IdentityStatus Pending = new("Pending");
    public static readonly IdentityStatus Active = new("Active");
    public static readonly IdentityStatus Suspended = new("Suspended");
    public static readonly IdentityStatus Deactivated = new("Deactivated");

    public override string ToString() => Value;
}
