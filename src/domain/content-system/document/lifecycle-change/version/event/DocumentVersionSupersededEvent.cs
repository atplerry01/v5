using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;

public sealed record DocumentVersionSupersededEvent(
    [property: JsonPropertyName("AggregateId")] DocumentVersionId VersionId,
    DocumentVersionId SuccessorVersionId,
    Timestamp SupersededAt) : DomainEvent;
