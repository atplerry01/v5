using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;

public sealed record PlaybackResumedEvent(
    [property: JsonPropertyName("AggregateId")] ProgressId ProgressId,
    Timestamp ResumedAt) : DomainEvent;
