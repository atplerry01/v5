namespace Whycespace.Engines.T0U.WhyceId.Verification;

/// <summary>
/// Represents the verification state of an identity.
/// </summary>
public enum VerificationStatus
{
    Unverified = 0,
    Pending = 1,
    Verified = 2,
    Revoked = 3
}
