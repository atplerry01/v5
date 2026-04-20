using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed record ChannelCreatedEvent(
    ChannelId ChannelId,
    StreamRef StreamRef,
    ChannelName Name,
    ChannelMode Mode,
    Timestamp CreatedAt) : DomainEvent;
