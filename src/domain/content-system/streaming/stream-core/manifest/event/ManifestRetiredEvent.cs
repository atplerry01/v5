using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public sealed record ManifestRetiredEvent(
    [property: JsonPropertyName("AggregateId")] ManifestId ManifestId,
    string Reason,
    Timestamp RetiredAt) : DomainEvent;
