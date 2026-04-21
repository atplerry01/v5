using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public sealed record ManifestUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ManifestId ManifestId,
    ManifestVersion PreviousVersion,
    ManifestVersion NewVersion,
    Timestamp UpdatedAt) : DomainEvent;
