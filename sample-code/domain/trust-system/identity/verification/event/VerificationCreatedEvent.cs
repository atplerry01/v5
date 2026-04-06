namespace Whycespace.Domain.TrustSystem.Identity.Verification;

/// <summary>
/// Topic: whyce.identity.verification.created
/// </summary>
public sealed record VerificationCreatedEvent(
    Guid VerificationId,
    Guid IdentityId,
    string VerificationType,
    string Method) : DomainEvent;
