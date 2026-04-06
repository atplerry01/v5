namespace Whycespace.Engines.T0U.Governance.IdentityGovernance;

/// <summary>
/// Identity-specific governance workflow definitions.
/// Covers: proposal, sanction, consent lifecycle.
/// Stateless — no persistence, no domain imports.
/// </summary>
public static class IdentityGovernanceWorkflows
{
    /// <summary>
    /// Governance actions that require proposal + approval workflow.
    /// </summary>
    public static readonly IReadOnlySet<string> ProposalRequiredActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "identity.deactivate",
        "role.create",
        "role.deactivate",
        "permission.create",
        "permission.deactivate",
        "service-identity.decommission",
        "identity-graph.merge",
        "identity-graph.close",
        "access-profile.suspend"
    };

    /// <summary>
    /// Actions that trigger sanction evaluation.
    /// </summary>
    public static readonly IReadOnlySet<string> SanctionTriggerActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "identity.suspend",
        "device.block",
        "credential.revoke",
        "session.revoke"
    };

    /// <summary>
    /// Actions that require active consent.
    /// </summary>
    public static readonly IReadOnlySet<string> ConsentRequiredActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "credential.issue",
        "service-identity.register",
        "identity-graph.link",
        "device.register"
    };

    public static bool RequiresProposal(string action) => ProposalRequiredActions.Contains(action);

    public static bool TriggersSanction(string action) => SanctionTriggerActions.Contains(action);

    public static bool RequiresConsent(string action) => ConsentRequiredActions.Contains(action);
}
