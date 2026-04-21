using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;

public sealed record DocumentVersionActivatedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentVersionId VersionId,
    Timestamp ActivatedAt) : DomainEvent;
