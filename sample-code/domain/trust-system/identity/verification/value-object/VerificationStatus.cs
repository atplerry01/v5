namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed record VerificationStatus(string Value)
{
    public static readonly VerificationStatus Pending = new("Pending");
    public static readonly VerificationStatus Completed = new("Completed");
    public static readonly VerificationStatus Failed = new("Failed");
    public static readonly VerificationStatus Expired = new("Expired");

    public override string ToString() => Value;
}
