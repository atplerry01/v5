using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

public sealed record EntitlementInvalidatedEvent(
    [property: JsonPropertyName("AggregateId")] EntitlementHookId HookId,
    Timestamp InvalidatedAt) : DomainEvent;
