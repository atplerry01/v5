namespace Whycespace.Projections.Decision.Governance.Proposal;

public interface IProposalViewRepository
{
    Task SaveAsync(ProposalReadModel model, CancellationToken ct = default);
    Task<ProposalReadModel?> GetAsync(string id, CancellationToken ct = default);
}
