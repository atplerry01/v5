using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Intake.Upload;

/// Reference to the document the upload is bound to. Bare-id to avoid
/// cross-BC type imports per domain.guard.md rule 13.
public readonly record struct DocumentUploadSourceRef
{
    public Guid Value { get; }

    public DocumentUploadSourceRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentUploadSourceRef cannot be empty.");
        Value = value;
    }
}
