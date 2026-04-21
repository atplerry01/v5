using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed record ChannelCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ChannelId ChannelId,
    StreamRef StreamRef,
    ChannelName Name,
    ChannelMode Mode,
    Timestamp CreatedAt) : DomainEvent;
