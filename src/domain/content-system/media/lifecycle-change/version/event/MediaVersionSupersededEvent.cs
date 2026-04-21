using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

public sealed record MediaVersionSupersededEvent(
    [property: JsonPropertyName("AggregateId")] MediaVersionId VersionId,
    MediaVersionId SuccessorVersionId,
    Timestamp SupersededAt) : DomainEvent;
