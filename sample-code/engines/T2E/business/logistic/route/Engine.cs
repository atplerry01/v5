namespace Whycespace.Engines.T2E.Business.Logistic.Route;

public class RouteEngine
{
    private readonly RoutePolicyAdapter _policy;

    public RouteEngine(RoutePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RouteResult> ExecuteAsync(RouteCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RouteResult(true, "Executed");
    }
}
