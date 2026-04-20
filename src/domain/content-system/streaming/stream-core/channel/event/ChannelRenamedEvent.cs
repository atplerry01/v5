using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed record ChannelRenamedEvent(
    ChannelId ChannelId,
    ChannelName PreviousName,
    ChannelName NewName,
    Timestamp RenamedAt) : DomainEvent;
