using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;

/// Local reference to a document aggregate id from another bounded context.
/// Carries only the identifier — does not import the foreign DocumentId type
/// (per domain.guard.md rule 13: no cross-BC type references).
public readonly record struct DocumentRef
{
    public Guid Value { get; }

    public DocumentRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentRef cannot be empty.");
        Value = value;
    }
}
