using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public readonly record struct VideoDuration
{
    public long Milliseconds { get; }

    public VideoDuration(long milliseconds)
    {
        Guard.Against(milliseconds < 0, "VideoDuration cannot be negative.");
        Milliseconds = milliseconds;
    }

    public override string ToString() => $"{Milliseconds} ms";
}
