using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;

public sealed record AllocationCreatedEvent(
    [property: JsonPropertyName("AggregateId")] AllocationId AllocationId,
    ResourceId ResourceId,
    int RequestedCapacity) : DomainEvent;
