namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// A local WhyceID identity linked to one or more external identities via federation.
/// Manages federation links and credentials across external issuers.
/// </summary>
public sealed class FederationIdentityAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public FederationStatus Status { get; private set; } = null!;

    private readonly List<FederationLink> _links = [];
    private readonly List<FederationCredential> _credentials = [];

    public IReadOnlyList<FederationLink> FederationLinks => _links.AsReadOnly();
    public IReadOnlyList<FederationCredential> Credentials => _credentials.AsReadOnly();

    private FederationIdentityAggregate() { }

    public static FederationIdentityAggregate Create(Guid identityId)
    {
        Guard.AgainstDefault(identityId);

        var agg = new FederationIdentityAggregate
        {
            IdentityId = identityId,
            Status = FederationStatus.Active
        };
        agg.Id = identityId;
        return agg;
    }

    /// <summary>
    /// Link this identity to an external identity at a federation issuer.
    /// Invariants: no duplicate active links for same externalId+issuerId.
    /// </summary>
    public FederationLink LinkExternal(
        ExternalIdentityId externalId,
        IssuerId issuerId,
        ConfidenceLevel confidence,
        VerificationLevel verification,
        LinkProvenance provenance,
        DateTimeOffset linkedAt)
    {
        Guard.AgainstNull(externalId);
        Guard.AgainstNull(issuerId);
        Guard.AgainstNull(provenance);

        EnsureNotTerminal(Status, s => s.IsTerminal, "LinkExternal");

        // Invariant: no duplicate active link
        var existing = _links.FirstOrDefault(l =>
            l.ExternalId == externalId
            && l.IssuerId == issuerId
            && l.Status == FederationLinkStatus.Active);

        EnsureInvariant(
            existing is null,
            "NO_DUPLICATE_LINK",
            $"Active link already exists for external '{externalId}' from issuer '{issuerId}'.");

        var link = FederationLink.Create(externalId, issuerId, confidence, verification, provenance, linkedAt);
        _links.Add(link);

        RaiseDomainEvent(new IdentityLinkedEvent(
            IdentityId, externalId.Value, issuerId.Value,
            confidence.Value, verification.Value,
            provenance.Source.ToString(), provenance.EvidenceReference));

        return link;
    }

    /// <summary>
    /// Remove an active federation link.
    /// </summary>
    public void UnlinkExternal(ExternalIdentityId externalId, IssuerId issuerId)
    {
        Guard.AgainstNull(externalId);
        Guard.AgainstNull(issuerId);

        var link = _links.FirstOrDefault(l =>
            l.ExternalId == externalId
            && l.IssuerId == issuerId
            && l.Status == FederationLinkStatus.Active);

        EnsureInvariant(
            link is not null,
            "LINK_MUST_EXIST",
            $"No active link found for external '{externalId}' from issuer '{issuerId}'.");

        link!.Revoke();

        RaiseDomainEvent(new IdentityUnlinkedEvent(
            IdentityId, externalId.Value, issuerId.Value));
    }

    /// <summary>
    /// Receive a credential from a federation issuer.
    /// </summary>
    public FederationCredential ReceiveCredential(
        IssuerId issuerId,
        string credentialType,
        DateTimeOffset issuedAt,
        DateTimeOffset? expiresAt = null)
    {
        EnsureNotTerminal(Status, s => s.IsTerminal, "ReceiveCredential");

        var credential = FederationCredential.Create(issuerId, credentialType, issuedAt, expiresAt);
        _credentials.Add(credential);

        RaiseDomainEvent(new CredentialReceivedEvent(
            credential.CredentialId, issuerId.Value, credentialType));

        return credential;
    }

    /// <summary>
    /// Revoke a credential.
    /// </summary>
    public void RevokeCredential(Guid credentialId)
    {
        var credential = _credentials.FirstOrDefault(c => c.CredentialId == credentialId && !c.Revoked);

        EnsureInvariant(
            credential is not null,
            "CREDENTIAL_MUST_EXIST",
            $"No active credential found with id '{credentialId}'.");

        credential!.Revoke();

        RaiseDomainEvent(new CredentialRevokedEvent(
            credentialId, credential.IssuerId.Value));
    }

    public void Suspend()
    {
        EnsureValidTransition(Status, FederationStatus.Suspended, FederationStatus.IsValidTransition);
        Status = FederationStatus.Suspended;
    }

    public void Revoke()
    {
        EnsureValidTransition(Status, FederationStatus.Revoked, FederationStatus.IsValidTransition);
        Status = FederationStatus.Revoked;

        // Revoke all active links
        foreach (var link in _links.Where(l => l.Status == FederationLinkStatus.Active))
            link.Revoke();
    }

    public void Reactivate()
    {
        EnsureValidTransition(Status, FederationStatus.Active, FederationStatus.IsValidTransition);
        Status = FederationStatus.Active;
    }
}
