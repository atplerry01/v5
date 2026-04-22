using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

public sealed record ReplayAbandonedEvent(
    [property: JsonPropertyName("AggregateId")] ReplayId ReplayId,
    Timestamp AbandonedAt) : DomainEvent;
