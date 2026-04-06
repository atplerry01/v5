namespace Whycespace.Engines.T2E.Business.Execution.Allocation;

public record AllocationCommand(
    string Action,
    string EntityId,
    object Payload
);
