namespace Whycespace.Shared.Contracts.Observability;

/// <summary>
/// Domain-level trust-system business metrics consumed by engines and
/// middleware without taking a reference on Runtime. Declared in Shared so
/// the engines layer can depend on it while Runtime provides the implementation.
///
/// Meter: Whycespace.Trust.Identity
/// </summary>
public interface ITrustMetrics
{
    // ── Registration lifecycle ─────────────────────────────────────────
    void RecordRegistrationInitiated(string registrationType);
    void RecordRegistrationVerified(string registrationType);
    void RecordRegistrationActivated(string registrationType);
    void RecordRegistrationRejected(string reason);
    void RecordRegistrationLocked(string reason);

    // ── Verification lifecycle ─────────────────────────────────────────
    void RecordVerificationInitiated(string verificationType);
    void RecordVerificationPassed(string verificationType);
    void RecordVerificationFailed(string verificationType);

    // ── Consent lifecycle ──────────────────────────────────────────────
    void RecordConsentGranted(string consentType);
    void RecordConsentRevoked(string consentType);
    void RecordConsentExpired(string consentType);

    // ── Session lifecycle ──────────────────────────────────────────────
    void RecordSessionOpened(string sessionType);
    void RecordSessionExpired(string sessionType);
    void RecordSessionTerminated(string reason);

    // ── Credential lifecycle ───────────────────────────────────────────
    void RecordCredentialIssued(string credentialType);
    void RecordCredentialRevoked(string reason);

    // ── Denial and failure signals ─────────────────────────────────────
    void RecordPolicyDenied(string policyId);
    void RecordThrottleViolation(string throttleKey);
    void RecordTokenFingerprintMismatch();
}
