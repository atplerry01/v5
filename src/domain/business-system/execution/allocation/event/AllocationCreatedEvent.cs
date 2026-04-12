namespace Whycespace.Domain.BusinessSystem.Execution.Allocation;

public sealed record AllocationCreatedEvent(AllocationId AllocationId, ExecutionResourceId ResourceId, int RequestedCapacity);
