namespace Whycespace.Domain.TrustSystem.Identity.Verification;

/// <summary>
/// Topic: whyce.identity.verification.failed
/// </summary>
public sealed record VerificationFailedEvent(
    Guid VerificationId,
    Guid IdentityId,
    string Reason) : DomainEvent;
