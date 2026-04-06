namespace Whycespace.Engines.T2E.Business.Resource.Utilization;

public record UtilizationCommand(
    string Action,
    string EntityId,
    object Payload
);
