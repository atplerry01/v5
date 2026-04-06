namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed record AccessLevel(string Value)
{
    public static readonly AccessLevel Basic = new("Basic");
    public static readonly AccessLevel Standard = new("Standard");
    public static readonly AccessLevel Elevated = new("Elevated");
    public static readonly AccessLevel Admin = new("Admin");

    public override string ToString() => Value;
}
