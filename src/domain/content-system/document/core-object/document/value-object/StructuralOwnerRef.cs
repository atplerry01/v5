using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

/// Structural parent of a content aggregate. Content never exists without a
/// structural owner (cluster / sub-cluster). Carries only the identifier —
/// does not import the foreign `ClusterId` type (per domain.guard.md rule 13).
public readonly record struct StructuralOwnerRef
{
    public Guid Value { get; }

    public StructuralOwnerRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "StructuralOwnerRef cannot be empty.");
        Value = value;
    }
}
