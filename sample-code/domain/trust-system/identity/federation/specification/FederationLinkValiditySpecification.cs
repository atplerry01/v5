namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Specification: Is a proposed federation link valid?
/// Rules:
///   - Cannot link the same external identity twice (active link already exists)
///   - Issuer must be approved
/// </summary>
public sealed class FederationLinkValiditySpecification
{
    public bool IsSatisfiedBy(
        FederationIdentityAggregate identity,
        ExternalIdentityId externalId,
        FederationIssuerAggregate issuer)
    {
        // Issuer must be approved
        if (!issuer.IsApproved)
            return false;

        // No duplicate active link
        var hasDuplicate = identity.FederationLinks.Any(l =>
            l.ExternalId == externalId
            && l.IssuerId == issuer.IssuerId
            && l.Status == FederationLinkStatus.Active);

        if (hasDuplicate)
            return false;

        return true;
    }

    public string? GetRejectionReason(
        FederationIdentityAggregate identity,
        ExternalIdentityId externalId,
        FederationIssuerAggregate issuer)
    {
        if (!issuer.IsApproved)
            return $"Issuer '{issuer.IssuerId}' is not approved.";

        var hasDuplicate = identity.FederationLinks.Any(l =>
            l.ExternalId == externalId
            && l.IssuerId == issuer.IssuerId
            && l.Status == FederationLinkStatus.Active);

        if (hasDuplicate)
            return $"Active link already exists for external '{externalId}' from issuer '{issuer.IssuerId}'.";

        return null;
    }
}
