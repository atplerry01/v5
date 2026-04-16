using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Playback;

public sealed record PlaybackPosition : ValueObject
{
    public long Milliseconds { get; }
    private PlaybackPosition(long ms) => Milliseconds = ms;

    public static PlaybackPosition Zero { get; } = new(0);

    public static PlaybackPosition Create(long milliseconds)
    {
        if (milliseconds < 0) throw PlaybackErrors.InvalidPosition();
        return new PlaybackPosition(milliseconds);
    }

    public override string ToString() => $"{Milliseconds}ms";
}
