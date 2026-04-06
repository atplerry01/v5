namespace Whycespace.Domain.DecisionSystem.Governance.Delegation;

public static class DelegationErrors
{
    public const string SelfDelegation = "DELEGATION_SELF";
    public const string AlreadyRevoked = "DELEGATION_ALREADY_REVOKED";
    public const string InvalidTransition = "DELEGATION_INVALID_TRANSITION";
}
