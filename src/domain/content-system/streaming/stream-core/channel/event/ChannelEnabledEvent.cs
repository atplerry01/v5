using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed record ChannelEnabledEvent(
    ChannelId ChannelId,
    Timestamp EnabledAt) : DomainEvent;
