using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

/// Reference from a content aggregate to its currently-active version.
/// Nullable at construction — set only once a version is attached.
public readonly record struct CurrentVersionRef
{
    public Guid Value { get; }

    public CurrentVersionRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CurrentVersionRef cannot be empty.");
        Value = value;
    }
}
