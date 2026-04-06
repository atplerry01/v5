namespace Whycespace.Projections.Decision.Governance.Quorum;

public interface IQuorumViewRepository
{
    Task SaveAsync(QuorumReadModel model, CancellationToken ct = default);
    Task<QuorumReadModel?> GetAsync(string id, CancellationToken ct = default);
}
