namespace Whycespace.Projections.Intelligence.Geo.GeoIndex;

public interface IGeoIndexViewRepository
{
    Task SaveAsync(GeoIndexReadModel model, CancellationToken ct = default);
    Task<GeoIndexReadModel?> GetAsync(string id, CancellationToken ct = default);
}
