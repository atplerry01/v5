namespace Whycespace.Projections.Intelligence.Geo.Proximity;

public interface IProximityViewRepository
{
    Task SaveAsync(ProximityReadModel model, CancellationToken ct = default);
    Task<ProximityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
