namespace Whycespace.Domain.TrustSystem.Access.Permission;

public sealed record PermissionScope(string Value)
{
    public static readonly PermissionScope Global = new("Global");
    public static readonly PermissionScope Cluster = new("Cluster");
    public static readonly PermissionScope SpvLocal = new("SpvLocal");

    public override string ToString() => Value;
}
