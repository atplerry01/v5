using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;

public sealed record StreamFlaggedEvent(
    [property: JsonPropertyName("AggregateId")] ModerationId ModerationId,
    ModerationTargetRef TargetRef,
    string FlagReason,
    Timestamp FlaggedAt) : DomainEvent;
