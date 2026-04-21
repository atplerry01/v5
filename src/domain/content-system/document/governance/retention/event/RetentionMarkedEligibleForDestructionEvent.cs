using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

public sealed record RetentionMarkedEligibleForDestructionEvent(
    [property: JsonPropertyName("AggregateId")] RetentionId RetentionId,
    Timestamp MarkedAt) : DomainEvent;
