using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed record ChannelDisabledEvent(
    [property: JsonPropertyName("AggregateId")] ChannelId ChannelId,
    string Reason,
    Timestamp DisabledAt) : DomainEvent;
