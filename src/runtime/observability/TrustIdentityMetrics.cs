using System.Diagnostics.Metrics;
using Whycespace.Shared.Contracts.Observability;

namespace Whycespace.Runtime.Observability;

/// <summary>
/// Domain-level business metrics for the trust-system. Tracks identity and
/// access control lifecycle outcomes — registration, verification, consent,
/// session, credential, and denial signals.
///
/// All instruments use System.Diagnostics.Metrics (OpenTelemetry-compatible).
/// DI-registered as the singleton <see cref="ITrustMetrics"/>; engines depend
/// on the interface only so the engines layer does not reference Runtime
/// (RUNTIME-LAYER-PURITY-01).
///
/// Meter: Whycespace.Trust.Identity
/// </summary>
public sealed class TrustIdentityMetrics : ITrustMetrics
{
    private static readonly Meter TrustMeter = new("Whycespace.Trust.Identity", "1.0.0");

    // ── Registration lifecycle ─────────────────────────────────────────
    private static readonly Counter<long> RegistrationInitiated =
        TrustMeter.CreateCounter<long>("whyce.trust.registration.initiated", "registrations", "Registration initiations by actor type");
    private static readonly Counter<long> RegistrationVerified =
        TrustMeter.CreateCounter<long>("whyce.trust.registration.verified", "registrations", "Registrations verified");
    private static readonly Counter<long> RegistrationActivated =
        TrustMeter.CreateCounter<long>("whyce.trust.registration.activated", "registrations", "Registrations activated (identity created)");
    private static readonly Counter<long> RegistrationRejected =
        TrustMeter.CreateCounter<long>("whyce.trust.registration.rejected", "registrations", "Registrations rejected");
    private static readonly Counter<long> RegistrationLocked =
        TrustMeter.CreateCounter<long>("whyce.trust.registration.locked", "registrations", "Registrations locked by security enforcement");

    // ── Verification lifecycle ─────────────────────────────────────────
    private static readonly Counter<long> VerificationInitiated =
        TrustMeter.CreateCounter<long>("whyce.trust.verification.initiated", "verifications", "Verifications initiated");
    private static readonly Counter<long> VerificationPassed =
        TrustMeter.CreateCounter<long>("whyce.trust.verification.passed", "verifications", "Verifications passed");
    private static readonly Counter<long> VerificationFailed =
        TrustMeter.CreateCounter<long>("whyce.trust.verification.failed", "verifications", "Verifications failed");

    // ── Consent lifecycle ──────────────────────────────────────────────
    private static readonly Counter<long> ConsentGranted =
        TrustMeter.CreateCounter<long>("whyce.trust.consent.granted", "consents", "Consent grants recorded");
    private static readonly Counter<long> ConsentRevoked =
        TrustMeter.CreateCounter<long>("whyce.trust.consent.revoked", "consents", "Consent revocations recorded");
    private static readonly Counter<long> ConsentExpired =
        TrustMeter.CreateCounter<long>("whyce.trust.consent.expired", "consents", "Consents expired");

    // ── Session lifecycle ──────────────────────────────────────────────
    private static readonly Counter<long> SessionOpened =
        TrustMeter.CreateCounter<long>("whyce.trust.session.opened", "sessions", "Sessions opened");
    private static readonly Counter<long> SessionExpired =
        TrustMeter.CreateCounter<long>("whyce.trust.session.expired", "sessions", "Sessions expired");
    private static readonly Counter<long> SessionTerminated =
        TrustMeter.CreateCounter<long>("whyce.trust.session.terminated", "sessions", "Sessions terminated");

    // ── Credential lifecycle ───────────────────────────────────────────
    private static readonly Counter<long> CredentialIssued =
        TrustMeter.CreateCounter<long>("whyce.trust.credential.issued", "credentials", "Credentials issued");
    private static readonly Counter<long> CredentialRevoked =
        TrustMeter.CreateCounter<long>("whyce.trust.credential.revoked", "credentials", "Credentials revoked");

    // ── Denial and anomaly signals ─────────────────────────────────────
    private static readonly Counter<long> PolicyDenied =
        TrustMeter.CreateCounter<long>("whyce.trust.policy.denied", "denials", "Commands denied by policy evaluation");
    private static readonly Counter<long> ThrottleViolation =
        TrustMeter.CreateCounter<long>("whyce.trust.throttle.violation", "violations", "Identity operations blocked by throttle policy");
    private static readonly Counter<long> TokenFingerprintMismatch =
        TrustMeter.CreateCounter<long>("whyce.trust.token.fingerprint_mismatch", "anomalies", "Token fingerprint mismatches on authenticated requests");

    // ── ITrustMetrics implementation ───────────────────────────────────

    public void RecordRegistrationInitiated(string registrationType) =>
        RegistrationInitiated.Add(1, new KeyValuePair<string, object?>("registration_type", registrationType));

    public void RecordRegistrationVerified(string registrationType) =>
        RegistrationVerified.Add(1, new KeyValuePair<string, object?>("registration_type", registrationType));

    public void RecordRegistrationActivated(string registrationType) =>
        RegistrationActivated.Add(1, new KeyValuePair<string, object?>("registration_type", registrationType));

    public void RecordRegistrationRejected(string reason) =>
        RegistrationRejected.Add(1, new KeyValuePair<string, object?>("reason", reason));

    public void RecordRegistrationLocked(string reason) =>
        RegistrationLocked.Add(1, new KeyValuePair<string, object?>("reason", reason));

    public void RecordVerificationInitiated(string verificationType) =>
        VerificationInitiated.Add(1, new KeyValuePair<string, object?>("verification_type", verificationType));

    public void RecordVerificationPassed(string verificationType) =>
        VerificationPassed.Add(1, new KeyValuePair<string, object?>("verification_type", verificationType));

    public void RecordVerificationFailed(string verificationType) =>
        VerificationFailed.Add(1, new KeyValuePair<string, object?>("verification_type", verificationType));

    public void RecordConsentGranted(string consentType) =>
        ConsentGranted.Add(1, new KeyValuePair<string, object?>("consent_type", consentType));

    public void RecordConsentRevoked(string consentType) =>
        ConsentRevoked.Add(1, new KeyValuePair<string, object?>("consent_type", consentType));

    public void RecordConsentExpired(string consentType) =>
        ConsentExpired.Add(1, new KeyValuePair<string, object?>("consent_type", consentType));

    public void RecordSessionOpened(string sessionType) =>
        SessionOpened.Add(1, new KeyValuePair<string, object?>("session_type", sessionType));

    public void RecordSessionExpired(string sessionType) =>
        SessionExpired.Add(1, new KeyValuePair<string, object?>("session_type", sessionType));

    public void RecordSessionTerminated(string reason) =>
        SessionTerminated.Add(1, new KeyValuePair<string, object?>("reason", reason));

    public void RecordCredentialIssued(string credentialType) =>
        CredentialIssued.Add(1, new KeyValuePair<string, object?>("credential_type", credentialType));

    public void RecordCredentialRevoked(string reason) =>
        CredentialRevoked.Add(1, new KeyValuePair<string, object?>("reason", reason));

    public void RecordPolicyDenied(string policyId) =>
        PolicyDenied.Add(1, new KeyValuePair<string, object?>("policy_id", policyId));

    public void RecordThrottleViolation(string throttleKey) =>
        ThrottleViolation.Add(1, new KeyValuePair<string, object?>("key", throttleKey));

    public void RecordTokenFingerprintMismatch() =>
        TokenFingerprintMismatch.Add(1);
}
