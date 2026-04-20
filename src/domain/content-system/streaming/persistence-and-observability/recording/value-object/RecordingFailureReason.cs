using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Recording;

public readonly record struct RecordingFailureReason
{
    public string Value { get; }

    public RecordingFailureReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "RecordingFailureReason cannot be empty.");
        Guard.Against(value.Length > 1024, "RecordingFailureReason cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
