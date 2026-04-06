namespace Whycespace.Projections.Decision.Governance.Vote;

public interface IVoteViewRepository
{
    Task SaveAsync(VoteReadModel model, CancellationToken ct = default);
    Task<VoteReadModel?> GetAsync(string id, CancellationToken ct = default);
}
