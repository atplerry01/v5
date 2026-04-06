namespace Whycespace.Projections.Intelligence.Geo.Distance;

public interface IDistanceViewRepository
{
    Task SaveAsync(DistanceReadModel model, CancellationToken ct = default);
    Task<DistanceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
