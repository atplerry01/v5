using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

/// Bare-id reference (with discriminator) to the source the manifest is rendered for.
/// Avoids cross-BC type imports per domain.guard.md rule 13.
public readonly record struct ManifestSourceRef
{
    public Guid Value { get; }
    public ManifestSourceKind Kind { get; }

    public ManifestSourceRef(Guid value, ManifestSourceKind kind)
    {
        Guard.Against(value == Guid.Empty, "ManifestSourceRef value cannot be empty.");
        Value = value;
        Kind = kind;
    }
}
