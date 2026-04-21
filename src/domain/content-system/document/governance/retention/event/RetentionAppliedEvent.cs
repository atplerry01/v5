using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

public sealed record RetentionAppliedEvent(
    [property: JsonPropertyName("AggregateId")] RetentionId RetentionId,
    RetentionTargetRef TargetRef,
    RetentionWindow Window,
    RetentionReason Reason,
    Timestamp AppliedAt) : DomainEvent;
