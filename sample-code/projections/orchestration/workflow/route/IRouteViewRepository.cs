namespace Whycespace.Projections.Orchestration.Workflow.Route;

public interface IRouteViewRepository
{
    Task SaveAsync(RouteReadModel model, CancellationToken ct = default);
    Task<RouteReadModel?> GetAsync(string id, CancellationToken ct = default);
}
