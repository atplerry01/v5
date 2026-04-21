using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public sealed record ManifestArchivedEvent(
    [property: JsonPropertyName("AggregateId")] ManifestId ManifestId,
    Timestamp ArchivedAt) : DomainEvent;
