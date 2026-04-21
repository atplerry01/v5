using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed record SessionOpenedEvent(
    [property: JsonPropertyName("AggregateId")] SessionId SessionId,
    StreamRef StreamRef,
    SessionWindow Window,
    Timestamp OpenedAt) : DomainEvent;
