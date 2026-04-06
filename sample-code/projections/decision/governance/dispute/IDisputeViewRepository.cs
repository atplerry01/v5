namespace Whycespace.Projections.Decision.Governance.Dispute;

public interface IDisputeViewRepository
{
    Task SaveAsync(DisputeReadModel model, CancellationToken ct = default);
    Task<DisputeReadModel?> GetAsync(string id, CancellationToken ct = default);
}
