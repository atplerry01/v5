namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed record ServiceIdentityStatus(string Value)
{
    public static readonly ServiceIdentityStatus Active = new("Active");
    public static readonly ServiceIdentityStatus Suspended = new("Suspended");
    public static readonly ServiceIdentityStatus Decommissioned = new("Decommissioned");

    public override string ToString() => Value;
}
