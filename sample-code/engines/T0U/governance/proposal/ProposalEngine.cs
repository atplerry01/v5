namespace Whycespace.Engines.T0U.Governance.Proposal;

public sealed class ProposalEngine : GovernanceEngineBase
{
    public ProposalResult Submit(SubmitProposalCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new ProposalResult(command.ProposalId, false);
    }
}
