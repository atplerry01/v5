using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;

public sealed record ModerationAssignedEvent(
    [property: JsonPropertyName("AggregateId")] ModerationId ModerationId,
    ModeratorRef Moderator,
    Timestamp AssignedAt) : DomainEvent;
