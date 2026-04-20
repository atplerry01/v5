using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Processing;

public readonly record struct MediaProcessingFailureReason
{
    public string Value { get; }

    public MediaProcessingFailureReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MediaProcessingFailureReason cannot be empty.");
        Guard.Against(value.Length > 1024, "MediaProcessingFailureReason cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
