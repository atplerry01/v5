using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Upload;

public readonly record struct DocumentUploadId
{
    public Guid Value { get; }

    public DocumentUploadId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentUploadId cannot be empty.");
        Value = value;
    }
}
