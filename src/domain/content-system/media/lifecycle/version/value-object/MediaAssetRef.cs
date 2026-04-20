using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Version;

/// Reference to a media/content-artifact/asset aggregate id, carried as a
/// bare id to avoid cross-BC type imports per domain.guard.md rule 13.
public readonly record struct MediaAssetRef
{
    public Guid Value { get; }

    public MediaAssetRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaAssetRef cannot be empty.");
        Value = value;
    }
}
