namespace Whycespace.Engines.T0U.Governance.Quorum;

public sealed class QuorumEngine : GovernanceEngineBase
{
    public QuorumResult Check(CheckQuorumCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new QuorumResult(command.ProposalId, false, 0, 0);
    }
}
