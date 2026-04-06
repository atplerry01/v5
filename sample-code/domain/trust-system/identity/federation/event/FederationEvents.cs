namespace Whycespace.Domain.TrustSystem.Identity.Federation;

// -- Issuer lifecycle --

// whyce.identity-federation.issuer.registered
public sealed record IssuerRegisteredEvent(
    Guid IssuerId, string Name, string IssuerType) : DomainEvent;

// whyce.identity-federation.issuer.approved
public sealed record IssuerApprovedEvent(
    Guid IssuerId, DateTimeOffset ApprovedAt) : DomainEvent;

// whyce.identity-federation.issuer.suspended
public sealed record IssuerSuspendedEvent(
    Guid IssuerId, string Reason) : DomainEvent;

// whyce.identity-federation.issuer.revoked
public sealed record IssuerRevokedEvent(
    Guid IssuerId, string Reason) : DomainEvent;

// -- Identity linking (includes ConfidenceProfile + Provenance) --

// whyce.identity-federation.identity.linked
public sealed record IdentityLinkedEvent(
    Guid IdentityId,
    string ExternalId,
    Guid IssuerId,
    decimal InitialConfidence,
    int VerificationLevel,
    string ProvenanceSource,
    string? EvidenceReference) : DomainEvent;

// whyce.identity-federation.identity.unlinked
public sealed record IdentityUnlinkedEvent(
    Guid IdentityId, string ExternalId, Guid IssuerId) : DomainEvent;

// -- Credential lifecycle --

// whyce.identity-federation.credential.received
public sealed record CredentialReceivedEvent(
    Guid CredentialId, Guid IssuerId, string CredentialType) : DomainEvent;

// whyce.identity-federation.credential.revoked
public sealed record CredentialRevokedEvent(
    Guid CredentialId, Guid IssuerId) : DomainEvent;

// -- Trust evaluation (includes TrustTrajectory) --

// whyce.identity-federation.trust.evaluated
public sealed record TrustEvaluatedEvent(
    Guid IssuerId,
    decimal BaseTrustScore,
    decimal AdjustedTrustScore,
    string Trend,
    decimal Volatility) : DomainEvent;

// whyce.identity-federation.trust.degraded
public sealed record TrustDegradedEvent(
    Guid IssuerId, decimal PreviousScore,
    decimal NewScore, string Reason) : DomainEvent;
