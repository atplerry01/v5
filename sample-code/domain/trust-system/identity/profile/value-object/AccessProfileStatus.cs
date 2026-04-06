namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed record AccessProfileStatus(string Value)
{
    public static readonly AccessProfileStatus Active = new("Active");
    public static readonly AccessProfileStatus Suspended = new("Suspended");

    public override string ToString() => Value;
}
