using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed record ChannelEnabledEvent(
    [property: JsonPropertyName("AggregateId")] ChannelId ChannelId,
    Timestamp EnabledAt) : DomainEvent;
