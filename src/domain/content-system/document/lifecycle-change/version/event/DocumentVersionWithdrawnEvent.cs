using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;

public sealed record DocumentVersionWithdrawnEvent(
    [property: JsonPropertyName("AggregateId")] DocumentVersionId VersionId,
    string Reason,
    Timestamp WithdrawnAt) : DomainEvent;
