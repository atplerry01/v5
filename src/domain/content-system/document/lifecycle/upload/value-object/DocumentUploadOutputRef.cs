using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Upload;

public readonly record struct DocumentUploadOutputRef
{
    public Guid Value { get; }

    public DocumentUploadOutputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentUploadOutputRef cannot be empty.");
        Value = value;
    }
}
