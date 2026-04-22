using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;

public sealed record ModerationOverturnedEvent(
    [property: JsonPropertyName("AggregateId")] ModerationId ModerationId,
    string Rationale,
    Timestamp OverturnedAt) : DomainEvent;
