namespace Whycespace.Engines.T2E.Business.Logistic.Route;

public record RouteCommand(
    string Action,
    string EntityId,
    object Payload
);
