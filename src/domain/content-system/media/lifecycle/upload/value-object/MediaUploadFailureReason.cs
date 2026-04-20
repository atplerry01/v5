using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public readonly record struct MediaUploadFailureReason
{
    public string Value { get; }

    public MediaUploadFailureReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MediaUploadFailureReason cannot be empty.");
        Guard.Against(value.Length > 1024, "MediaUploadFailureReason cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
