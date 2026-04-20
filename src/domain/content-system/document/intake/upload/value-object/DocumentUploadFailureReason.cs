using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Intake.Upload;

public readonly record struct DocumentUploadFailureReason
{
    public string Value { get; }

    public DocumentUploadFailureReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "DocumentUploadFailureReason cannot be empty.");
        Guard.Against(value.Length > 1024, "DocumentUploadFailureReason cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
