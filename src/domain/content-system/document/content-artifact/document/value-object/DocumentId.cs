using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Document;

public readonly record struct DocumentId
{
    public Guid Value { get; }

    public DocumentId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentId cannot be empty.");
        Value = value;
    }
}
