using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

public sealed record EntitlementHookRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] EntitlementHookId HookId,
    EntitlementTargetRef TargetRef,
    SourceSystemRef SourceSystem,
    Timestamp RegisteredAt) : DomainEvent;
