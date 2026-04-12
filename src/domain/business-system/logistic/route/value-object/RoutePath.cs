namespace Whycespace.Domain.BusinessSystem.Logistic.Route;

public sealed class RoutePath
{
    private readonly IReadOnlyList<Waypoint> _waypoints;

    public IReadOnlyList<Waypoint> Waypoints => _waypoints;

    public RoutePath(IReadOnlyList<Waypoint> waypoints)
    {
        if (waypoints is null || waypoints.Count < 2)
            throw new ArgumentException("RoutePath must contain at least two waypoints.", nameof(waypoints));

        _waypoints = waypoints;
    }
}
