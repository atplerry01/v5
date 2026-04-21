using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed record SessionFailedEvent(
    [property: JsonPropertyName("AggregateId")] SessionId SessionId,
    SessionTerminationReason Reason,
    Timestamp FailedAt) : DomainEvent;
