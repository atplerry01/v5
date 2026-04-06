namespace Whycespace.Domain.DecisionSystem.Governance.ClusterDecision;

public sealed class ClusterGovernanceException : DomainException
{
    public ClusterGovernanceException(string message) : base("CLUSTER_GOVERNANCE_ERROR", message) { }
}

public static class ClusterGovernanceErrors
{
    public const string InvalidTransition = "CLUSTER_GOVERNANCE_INVALID_TRANSITION";
    public const string NotApproved = "CLUSTER_GOVERNANCE_NOT_APPROVED";
    public const string AlreadyTerminal = "CLUSTER_GOVERNANCE_ALREADY_TERMINAL";
}
