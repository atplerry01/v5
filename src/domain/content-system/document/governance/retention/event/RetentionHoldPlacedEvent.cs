using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

public sealed record RetentionHoldPlacedEvent(
    [property: JsonPropertyName("AggregateId")] RetentionId RetentionId,
    RetentionReason Reason,
    Timestamp PlacedAt) : DomainEvent;
