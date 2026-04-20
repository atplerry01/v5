using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public readonly record struct ChannelId
{
    public Guid Value { get; }

    public ChannelId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ChannelId cannot be empty.");
        Value = value;
    }
}
