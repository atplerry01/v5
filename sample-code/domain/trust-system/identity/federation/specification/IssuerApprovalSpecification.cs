namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Specification: Can an issuer be approved?
/// Rules:
///   - Cannot approve an already-approved issuer
///   - Must meet minimum trust threshold
/// </summary>
public sealed class IssuerApprovalSpecification
{
    private readonly decimal _minimumTrustThreshold;

    public IssuerApprovalSpecification(decimal minimumTrustThreshold = 25m)
    {
        _minimumTrustThreshold = minimumTrustThreshold;
    }

    public bool IsSatisfiedBy(FederationIssuerAggregate issuer)
    {
        if (issuer.Status == IssuerStatus.Approved)
            return false;

        if (issuer.Status == IssuerStatus.Revoked)
            return false;

        if (!issuer.TrustLevel.MeetsThreshold(_minimumTrustThreshold))
            return false;

        return true;
    }

    public string? GetRejectionReason(FederationIssuerAggregate issuer)
    {
        if (issuer.Status == IssuerStatus.Approved)
            return "Issuer is already approved.";

        if (issuer.Status == IssuerStatus.Revoked)
            return "Revoked issuers cannot be approved.";

        if (!issuer.TrustLevel.MeetsThreshold(_minimumTrustThreshold))
            return $"Trust level {issuer.TrustLevel.Value} is below minimum threshold {_minimumTrustThreshold}.";

        return null;
    }
}
