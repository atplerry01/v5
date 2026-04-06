namespace Whycespace.Engines.T2E.Business.Resource.MaintenanceResource;

public record MaintenanceResourceCommand(
    string Action,
    string EntityId,
    object Payload
);
