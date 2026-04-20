using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Audio;

public readonly record struct AudioDuration
{
    public long Milliseconds { get; }

    public AudioDuration(long milliseconds)
    {
        Guard.Against(milliseconds < 0, "AudioDuration cannot be negative.");
        Milliseconds = milliseconds;
    }

    public override string ToString() => $"{Milliseconds} ms";
}
