using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

public sealed record EntitlementFailureRecordedEvent(
    [property: JsonPropertyName("AggregateId")] EntitlementHookId HookId,
    string Reason,
    Timestamp FailedAt) : DomainEvent;
