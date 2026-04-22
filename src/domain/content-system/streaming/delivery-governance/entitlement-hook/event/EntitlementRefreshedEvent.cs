using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

public sealed record EntitlementRefreshedEvent(
    [property: JsonPropertyName("AggregateId")] EntitlementHookId HookId,
    EntitlementStatus Result,
    Timestamp RefreshedAt) : DomainEvent;
