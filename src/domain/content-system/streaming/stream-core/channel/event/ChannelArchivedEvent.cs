using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed record ChannelArchivedEvent(
    ChannelId ChannelId,
    Timestamp ArchivedAt) : DomainEvent;
