using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

public sealed record ReplayResumedEvent(
    [property: JsonPropertyName("AggregateId")] ReplayId ReplayId,
    Timestamp ResumedAt) : DomainEvent;
