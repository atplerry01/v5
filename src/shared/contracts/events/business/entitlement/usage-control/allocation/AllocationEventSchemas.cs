namespace Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.Allocation;

public sealed record AllocationCreatedEventSchema(Guid AggregateId, Guid ResourceId, int RequestedCapacity);

public sealed record AllocationAllocatedEventSchema(Guid AggregateId);

public sealed record AllocationReleasedEventSchema(Guid AggregateId);
