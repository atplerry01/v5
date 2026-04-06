namespace Whycespace.Projections.Global.SystemHealth;

public interface IGlobalSystemHealthViewRepository
{
    Task SaveAsync(GlobalSystemHealthReadModel model, CancellationToken ct = default);
    Task<GlobalSystemHealthReadModel?> GetAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<GlobalSystemHealthReadModel>> GetByRegionAsync(string regionId, CancellationToken ct = default);
}
