using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

public sealed record EntitlementQueriedEvent(
    [property: JsonPropertyName("AggregateId")] EntitlementHookId HookId,
    EntitlementStatus Result,
    Timestamp QueriedAt) : DomainEvent;
