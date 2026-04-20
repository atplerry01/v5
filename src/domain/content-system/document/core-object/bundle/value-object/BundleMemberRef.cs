using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

/// Reference to a member document or document-file artifact carried as a
/// bare id. Avoids cross-BC type imports per domain.guard.md rule 13.
public readonly record struct BundleMemberRef
{
    public Guid Value { get; }

    public BundleMemberRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "BundleMemberRef cannot be empty.");
        Value = value;
    }
}
