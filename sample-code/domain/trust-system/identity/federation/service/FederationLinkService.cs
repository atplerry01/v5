namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Domain service for federation identity linking.
/// Coordinates link operations across identity and issuer aggregates.
///
/// Rules:
///   - Issuer MUST be approved
///   - Link MUST be unique (no duplicate active links)
///   - Confidence MUST be provided
///   - NO external calls, NO persistence, NO randomness
/// </summary>
public static class FederationLinkService
{
    /// <summary>
    /// Link a local identity to an external identity at an approved issuer.
    /// </summary>
    public static FederationLink LinkIdentity(
        FederationIdentityAggregate identity,
        ExternalIdentityId externalId,
        FederationIssuerAggregate issuer,
        ConfidenceLevel confidence,
        VerificationLevel verification,
        LinkProvenance provenance,
        DateTimeOffset linkedAt)
    {
        Guard.AgainstNull(identity);
        Guard.AgainstNull(externalId);
        Guard.AgainstNull(issuer);
        Guard.AgainstNull(confidence);
        Guard.AgainstNull(verification);
        Guard.AgainstNull(provenance);

        // Issuer must be approved
        if (!issuer.IsApproved)
            throw FederationErrors.IssuerNotApproved(issuer.IssuerId.Value);

        // Delegate to aggregate (enforces duplicate invariant + provenance)
        return identity.LinkExternal(externalId, issuer.IssuerId, confidence, verification, provenance, linkedAt);
    }

    /// <summary>
    /// Remove (revoke) an active federation link.
    /// </summary>
    public static void UnlinkIdentity(
        FederationIdentityAggregate identity,
        ExternalIdentityId externalId,
        IssuerId issuerId)
    {
        Guard.AgainstNull(identity);
        Guard.AgainstNull(externalId);
        Guard.AgainstNull(issuerId);

        var link = identity.FederationLinks.FirstOrDefault(l =>
            l.ExternalId == externalId
            && l.IssuerId == issuerId
            && l.Status == FederationLinkStatus.Active);

        if (link is null)
            throw FederationErrors.FederationLinkNotFound(externalId.Value, issuerId.Value);

        identity.UnlinkExternal(externalId, issuerId);
    }
}
