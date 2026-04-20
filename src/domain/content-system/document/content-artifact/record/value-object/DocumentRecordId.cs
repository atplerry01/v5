using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Record;

public readonly record struct DocumentRecordId
{
    public Guid Value { get; }

    public DocumentRecordId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentRecordId cannot be empty.");
        Value = value;
    }
}
