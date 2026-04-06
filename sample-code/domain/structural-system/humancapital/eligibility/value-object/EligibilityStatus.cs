namespace Whycespace.Domain.StructuralSystem.HumanCapital.Eligibility;

public sealed record EligibilityStatus(string Value)
{
    public static readonly EligibilityStatus Granted = new("granted");
    public static readonly EligibilityStatus Revoked = new("revoked");
    public static readonly EligibilityStatus Pending = new("pending");
}
