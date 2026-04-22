namespace Whycespace.Shared.Contracts.Observability;

/// <summary>
/// No-op <see cref="ITrustMetrics"/> used in tests and non-production environments
/// where metric emission is not required.
/// </summary>
public sealed class NoOpTrustMetrics : ITrustMetrics
{
    public static readonly ITrustMetrics Instance = new NoOpTrustMetrics();

    public void RecordRegistrationInitiated(string registrationType) { }
    public void RecordRegistrationVerified(string registrationType) { }
    public void RecordRegistrationActivated(string registrationType) { }
    public void RecordRegistrationRejected(string reason) { }
    public void RecordRegistrationLocked(string reason) { }

    public void RecordVerificationInitiated(string verificationType) { }
    public void RecordVerificationPassed(string verificationType) { }
    public void RecordVerificationFailed(string verificationType) { }

    public void RecordConsentGranted(string consentType) { }
    public void RecordConsentRevoked(string consentType) { }
    public void RecordConsentExpired(string consentType) { }

    public void RecordSessionOpened(string sessionType) { }
    public void RecordSessionExpired(string sessionType) { }
    public void RecordSessionTerminated(string reason) { }

    public void RecordCredentialIssued(string credentialType) { }
    public void RecordCredentialRevoked(string reason) { }

    public void RecordPolicyDenied(string policyId) { }
    public void RecordThrottleViolation(string throttleKey) { }
    public void RecordTokenFingerprintMismatch() { }
}
