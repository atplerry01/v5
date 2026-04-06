namespace Whycespace.Projections.Decision.Audit.Finding;

public interface IFindingViewRepository
{
    Task SaveAsync(FindingReadModel model, CancellationToken ct = default);
    Task<FindingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
