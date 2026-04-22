using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

public readonly record struct PlaybackPosition
{
    public long Milliseconds { get; }

    public PlaybackPosition(long milliseconds)
    {
        Guard.Against(milliseconds < 0, "PlaybackPosition cannot be negative.");
        Milliseconds = milliseconds;
    }
}
