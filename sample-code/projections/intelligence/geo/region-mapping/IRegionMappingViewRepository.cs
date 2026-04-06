namespace Whycespace.Projections.Intelligence.Geo.RegionMapping;

public interface IRegionMappingViewRepository
{
    Task SaveAsync(RegionMappingReadModel model, CancellationToken ct = default);
    Task<RegionMappingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
