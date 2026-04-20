using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Playback;

/// Reference to the playback source (a stream, a stream-session, or a recording),
/// carried as a bare id with a discriminator. Avoids cross-BC type imports
/// per domain.guard.md rule 13.
public readonly record struct PlaybackSourceRef
{
    public Guid Value { get; }
    public PlaybackSourceKind Kind { get; }

    public PlaybackSourceRef(Guid value, PlaybackSourceKind kind)
    {
        Guard.Against(value == Guid.Empty, "PlaybackSourceRef value cannot be empty.");
        Value = value;
        Kind = kind;
    }
}
