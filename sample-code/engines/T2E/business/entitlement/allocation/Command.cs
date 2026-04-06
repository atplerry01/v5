namespace Whycespace.Engines.T2E.Business.Entitlement.Allocation;

public record AllocationCommand(
    string Action,
    string EntityId,
    object Payload
);
