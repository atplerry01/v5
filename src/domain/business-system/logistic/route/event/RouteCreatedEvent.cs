namespace Whycespace.Domain.BusinessSystem.Logistic.Route;

public sealed record RouteCreatedEvent(
    RouteId RouteId,
    RoutePath Path);
