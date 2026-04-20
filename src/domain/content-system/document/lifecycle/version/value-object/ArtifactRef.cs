using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Version;

/// Local reference to a stored artifact (file/media-file) carried as a bare id.
/// Avoids cross-BC type import per domain.guard.md rule 13.
public readonly record struct ArtifactRef
{
    public Guid Value { get; }

    public ArtifactRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ArtifactRef cannot be empty.");
        Value = value;
    }
}
