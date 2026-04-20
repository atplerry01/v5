using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Upload;

public readonly record struct DocumentUploadInputRef
{
    public Guid Value { get; }

    public DocumentUploadInputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentUploadInputRef cannot be empty.");
        Value = value;
    }
}
