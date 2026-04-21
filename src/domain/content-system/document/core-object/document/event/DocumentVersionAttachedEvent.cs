using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public sealed record DocumentVersionAttachedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentId DocumentId,
    CurrentVersionRef VersionRef,
    Timestamp AttachedAt) : DomainEvent;
