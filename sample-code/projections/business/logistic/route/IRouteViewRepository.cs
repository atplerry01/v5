namespace Whycespace.Projections.Business.Logistic.Route;

public interface IRouteViewRepository
{
    Task SaveAsync(RouteReadModel model, CancellationToken ct = default);
    Task<RouteReadModel?> GetAsync(string id, CancellationToken ct = default);
}
