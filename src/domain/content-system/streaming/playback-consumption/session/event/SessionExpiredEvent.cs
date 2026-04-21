using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed record SessionExpiredEvent(
    [property: JsonPropertyName("AggregateId")] SessionId SessionId,
    Timestamp ExpiredAt) : DomainEvent;
