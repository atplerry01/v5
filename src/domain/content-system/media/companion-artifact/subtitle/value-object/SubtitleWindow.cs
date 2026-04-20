using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CompanionArtifact.Subtitle;

/// Optional applicability window for the subtitle within the parent media's
/// timeline. Encoded as media-time offsets (milliseconds), not wall-clock.
public readonly record struct SubtitleWindow
{
    public long StartMs { get; }
    public long EndMs { get; }

    public SubtitleWindow(long startMs, long endMs)
    {
        Guard.Against(startMs < 0, "SubtitleWindow startMs cannot be negative.");
        Guard.Against(endMs <= startMs, "SubtitleWindow endMs must be greater than startMs.");
        StartMs = startMs;
        EndMs = endMs;
    }

    public override string ToString() => $"[{StartMs}ms, {EndMs}ms]";
}
