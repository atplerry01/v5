using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Record;

public readonly record struct DocumentRef
{
    public Guid Value { get; }

    public DocumentRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentRef cannot be empty.");
        Value = value;
    }
}
