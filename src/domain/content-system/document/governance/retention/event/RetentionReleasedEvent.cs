using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

public sealed record RetentionReleasedEvent(
    [property: JsonPropertyName("AggregateId")] RetentionId RetentionId,
    Timestamp ReleasedAt) : DomainEvent;
