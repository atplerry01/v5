namespace Whycespace.Projections.Intelligence.Geo.Routing;

public interface IRoutingViewRepository
{
    Task SaveAsync(RoutingReadModel model, CancellationToken ct = default);
    Task<RoutingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
