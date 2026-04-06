namespace Whycespace.Domain.DecisionSystem.Compliance.Attestation;

public sealed record VerificationStatus(string Value)
{
    public static readonly VerificationStatus Pending = new("PENDING");
    public static readonly VerificationStatus UnderReview = new("UNDER_REVIEW");
    public static readonly VerificationStatus Verified = new("VERIFIED");
    public static readonly VerificationStatus Rejected = new("REJECTED");
    public static readonly VerificationStatus Suspended = new("SUSPENDED");
    public static readonly VerificationStatus Expired = new("EXPIRED");

    public bool IsTerminal => this == Rejected;
    public bool IsActive => this == Verified;
}
