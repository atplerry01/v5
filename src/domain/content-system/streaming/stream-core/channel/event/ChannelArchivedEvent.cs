using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed record ChannelArchivedEvent(
    [property: JsonPropertyName("AggregateId")] ChannelId ChannelId,
    Timestamp ArchivedAt) : DomainEvent;
