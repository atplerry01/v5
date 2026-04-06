namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed record IdentityType(string Value)
{
    public static readonly IdentityType Individual = new("Individual");
    public static readonly IdentityType Organization = new("Organization");
    public static readonly IdentityType Service = new("Service");
    public static readonly IdentityType System = new("System");

    public override string ToString() => Value;
}
