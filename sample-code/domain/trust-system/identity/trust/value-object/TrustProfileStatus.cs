namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed record TrustProfileStatus(string Value)
{
    public static readonly TrustProfileStatus Active = new("Active");
    public static readonly TrustProfileStatus Frozen = new("Frozen");

    public override string ToString() => Value;
}
