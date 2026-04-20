namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;

public sealed record AllocationCreatedEvent(AllocationId AllocationId, ResourceId ResourceId, int RequestedCapacity);