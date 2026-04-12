namespace Whycespace.Domain.BusinessSystem.Logistic.Route;

public static class RouteErrors
{
    public static RouteDomainException MissingId()
        => new("RouteId is required and must not be empty.");

    public static RouteDomainException PathRequired()
        => new("Route must have a defined path with at least two waypoints.");

    public static RouteDomainException InvalidStateTransition(RouteStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static RouteDomainException AlreadyLocked()
        => new("Route has been locked and cannot be modified.");
}

public sealed class RouteDomainException : Exception
{
    public RouteDomainException(string message) : base(message) { }
}
