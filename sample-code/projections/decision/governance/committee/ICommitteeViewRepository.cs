namespace Whycespace.Projections.Decision.Governance.Committee;

public interface ICommitteeViewRepository
{
    Task SaveAsync(CommitteeReadModel model, CancellationToken ct = default);
    Task<CommitteeReadModel?> GetAsync(string id, CancellationToken ct = default);
}
