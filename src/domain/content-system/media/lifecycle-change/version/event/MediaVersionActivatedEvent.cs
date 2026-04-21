using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

public sealed record MediaVersionActivatedEvent(
    [property: JsonPropertyName("AggregateId")] MediaVersionId VersionId,
    Timestamp ActivatedAt) : DomainEvent;
