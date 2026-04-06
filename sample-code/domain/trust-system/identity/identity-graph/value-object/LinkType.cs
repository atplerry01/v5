namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed record LinkType(string Value)
{
    public static readonly LinkType ParentChild = new("ParentChild");
    public static readonly LinkType Delegate = new("Delegate");
    public static readonly LinkType Alias = new("Alias");
    public static readonly LinkType ServiceOwner = new("ServiceOwner");
    public static readonly LinkType OrganizationMember = new("OrganizationMember");

    public override string ToString() => Value;
}
