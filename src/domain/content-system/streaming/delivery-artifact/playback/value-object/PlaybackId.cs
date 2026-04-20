using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Playback;

public readonly record struct PlaybackId
{
    public Guid Value { get; }

    public PlaybackId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PlaybackId cannot be empty.");
        Value = value;
    }
}
