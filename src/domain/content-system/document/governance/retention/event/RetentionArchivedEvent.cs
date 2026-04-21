using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

public sealed record RetentionArchivedEvent(
    [property: JsonPropertyName("AggregateId")] RetentionId RetentionId,
    Timestamp ArchivedAt) : DomainEvent;
