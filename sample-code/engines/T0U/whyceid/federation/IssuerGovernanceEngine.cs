namespace Whycespace.Engines.T0U.WhyceId.Federation;

/// <summary>
/// T0U Constitutional Engine — enforces issuer lifecycle governance rules.
///
/// Rules:
///   - Cannot approve a revoked issuer
///   - Cannot approve an already-approved issuer
///   - Approval requires minimum trust threshold (25)
///   - Cannot revoke an already-revoked issuer
///   - Suspension allowed only if approved
///
/// Stateless. No persistence. Deterministic.
/// Uses string-based status values instead of domain enums.
/// </summary>
public sealed class IssuerGovernanceEngine
{
    private const decimal MinimumApprovalTrust = 25m;

    public GovernanceDecision Approve(ApproveIssuerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.CurrentStatus == IssuerStatusValues.Revoked)
            return GovernanceDecision.Reject("approve",
                "Cannot approve a revoked issuer.");

        if (command.CurrentStatus == IssuerStatusValues.Approved)
            return GovernanceDecision.Reject("approve",
                "Issuer is already approved.");

        if (command.TrustLevel < MinimumApprovalTrust)
            return GovernanceDecision.Reject("approve",
                $"Trust level {command.TrustLevel:F1} is below minimum threshold {MinimumApprovalTrust}.");

        return GovernanceDecision.Allow("approve");
    }

    public GovernanceDecision Suspend(SuspendIssuerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.CurrentStatus != IssuerStatusValues.Approved)
            return GovernanceDecision.Reject("suspend",
                $"Can only suspend an approved issuer (current: {command.CurrentStatus}).");

        return GovernanceDecision.Allow("suspend");
    }

    public GovernanceDecision Revoke(RevokeIssuerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.CurrentStatus == IssuerStatusValues.Revoked)
            return GovernanceDecision.Reject("revoke",
                "Issuer is already revoked.");

        return GovernanceDecision.Allow("revoke");
    }
}
