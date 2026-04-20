using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

/// Reference to the registered media-file backing this version.
public readonly record struct MediaFileRef
{
    public Guid Value { get; }

    public MediaFileRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaFileRef cannot be empty.");
        Value = value;
    }
}
