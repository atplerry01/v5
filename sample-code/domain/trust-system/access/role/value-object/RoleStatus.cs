namespace Whycespace.Domain.TrustSystem.Access.Role;

public sealed record RoleStatus(string Value)
{
    public static readonly RoleStatus Active = new("Active");
    public static readonly RoleStatus Inactive = new("Inactive");

    public bool IsTerminal => this == Inactive;

    public override string ToString() => Value;
}
