namespace Whycespace.Projections.Decision.Governance.Resolution;

public interface IResolutionViewRepository
{
    Task SaveAsync(ResolutionReadModel model, CancellationToken ct = default);
    Task<ResolutionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
