namespace Whycespace.Domain.StructuralSystem.HumanCapital.Operator;

public sealed record AuthorizationLevel(string Value)
{
    public static readonly AuthorizationLevel Standard = new("standard");
    public static readonly AuthorizationLevel Elevated = new("elevated");
    public static readonly AuthorizationLevel Admin = new("admin");
}
