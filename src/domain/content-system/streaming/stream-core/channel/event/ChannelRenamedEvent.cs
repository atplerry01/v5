using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed record ChannelRenamedEvent(
    [property: JsonPropertyName("AggregateId")] ChannelId ChannelId,
    ChannelName PreviousName,
    ChannelName NewName,
    Timestamp RenamedAt) : DomainEvent;
