namespace Whycespace.Domain.DecisionSystem.Governance.Vote;

public static class VotingErrors
{
    public const string InvalidState = "BALLOT_INVALID_STATE";
    public const string AlreadyClosed = "BALLOT_ALREADY_CLOSED";
    public const string InvalidTransition = "BALLOT_INVALID_TRANSITION";
}
