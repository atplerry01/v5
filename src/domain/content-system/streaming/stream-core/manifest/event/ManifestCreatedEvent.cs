using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public sealed record ManifestCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ManifestId ManifestId,
    ManifestSourceRef SourceRef,
    ManifestVersion InitialVersion,
    Timestamp CreatedAt) : DomainEvent;
