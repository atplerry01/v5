namespace Whycespace.Domain.DecisionSystem.Governance.Proposal;

public static class ProposalErrors
{
    public const string InvalidState = "PROPOSAL_INVALID_STATE";
    public const string AlreadyApproved = "PROPOSAL_ALREADY_APPROVED";
    public const string AlreadyRejected = "PROPOSAL_ALREADY_REJECTED";
    public const string InvalidTransition = "PROPOSAL_INVALID_TRANSITION";
}
