namespace Whycespace.Projections.Structural.Cluster.Continuity;

public interface IContinuityViewRepository
{
    Task SaveAsync(ContinuityReadModel model, CancellationToken ct = default);
    Task<ContinuityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
