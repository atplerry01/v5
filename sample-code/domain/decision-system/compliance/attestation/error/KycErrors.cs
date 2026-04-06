namespace Whycespace.Domain.DecisionSystem.Compliance.Attestation;

public static class KycErrors
{
    public const string InvalidTransition = "KYC_INVALID_TRANSITION";
    public const string InvalidRejectionReason = "KYC_INVALID_REJECTION_REASON";
    public const string NotFound = "KYC_PROFILE_NOT_FOUND";
    public const string AlreadyVerified = "KYC_ALREADY_VERIFIED";
    public const string Expired = "KYC_PROFILE_EXPIRED";
    public const string IdentityMismatch = "KYC_IDENTITY_MISMATCH";
}
