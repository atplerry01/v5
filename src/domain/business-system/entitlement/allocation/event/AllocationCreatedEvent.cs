namespace Whycespace.Domain.BusinessSystem.Entitlement.Allocation;

public sealed record AllocationCreatedEvent(AllocationId AllocationId, ResourceId ResourceId, int RequestedCapacity);