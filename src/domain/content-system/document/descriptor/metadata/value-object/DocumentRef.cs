using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

/// Reference to a document/content-artifact/document aggregate id, carried as
/// a bare id to avoid cross-BC type imports per domain.guard.md rule 13.
public readonly record struct DocumentRef
{
    public Guid Value { get; }

    public DocumentRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentRef cannot be empty.");
        Value = value;
    }
}
