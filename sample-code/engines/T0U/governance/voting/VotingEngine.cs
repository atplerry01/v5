namespace Whycespace.Engines.T0U.Governance.Voting;

public sealed class VotingEngine : GovernanceEngineBase
{
    public VoteResult CastVote(CastVoteCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new VoteResult(command.ProposalId, command.VoterId, true);
    }
}
