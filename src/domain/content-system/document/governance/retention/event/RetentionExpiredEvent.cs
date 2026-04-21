using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

public sealed record RetentionExpiredEvent(
    [property: JsonPropertyName("AggregateId")] RetentionId RetentionId,
    Timestamp ExpiredAt) : DomainEvent;
