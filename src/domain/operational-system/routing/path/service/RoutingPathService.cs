namespace Whycespace.Domain.OperationalSystem.Routing.Path;

public sealed class RoutingPathService
{
    /// <summary>
    /// Validates that only active paths are eligible for routing decisions.
    /// </summary>
    public bool IsEligibleForRouting(RoutingPathAggregate path)
    {
        return path.Status == RoutingPathStatus.Active;
    }
}
