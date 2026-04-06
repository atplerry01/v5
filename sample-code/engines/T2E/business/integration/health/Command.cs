namespace Whycespace.Engines.T2E.Business.Integration.Health;

public record HealthCommand(
    string Action,
    string EntityId,
    object Payload
);
