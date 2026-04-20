using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed record ChannelDisabledEvent(
    ChannelId ChannelId,
    string Reason,
    Timestamp DisabledAt) : DomainEvent;
