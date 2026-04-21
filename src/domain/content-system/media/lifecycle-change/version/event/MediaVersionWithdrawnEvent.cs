using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

public sealed record MediaVersionWithdrawnEvent(
    [property: JsonPropertyName("AggregateId")] MediaVersionId VersionId,
    string Reason,
    Timestamp WithdrawnAt) : DomainEvent;
