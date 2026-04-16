using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Playback;

public static class PlaybackErrors
{
    public static DomainException InvalidPosition() => new("Playback position must be non-negative.");
    public static DomainException InvalidAssetRef() => new("Playback asset reference must be non-empty.");
    public static DomainException InvalidViewerRef() => new("Playback viewer reference must be non-empty.");
    public static DomainException InvalidTransition(PlaybackStatus from, PlaybackStatus to) =>
        new($"Illegal playback transition {from} -> {to}.");
    public static DomainException NotPaused() => new("Playback is not paused.");
    public static DomainException AlreadyTerminal() => new("Playback session has already terminated.");
    public static DomainInvariantViolationException AssetMissing() =>
        new("Invariant violated: playback session must reference an asset.");
}
