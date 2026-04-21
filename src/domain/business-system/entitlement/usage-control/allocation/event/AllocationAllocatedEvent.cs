using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;

public sealed record AllocationAllocatedEvent(
    [property: JsonPropertyName("AggregateId")] AllocationId AllocationId) : DomainEvent;
