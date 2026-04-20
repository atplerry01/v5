using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public readonly record struct ArchiveFailureReason
{
    public string Value { get; }

    public ArchiveFailureReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ArchiveFailureReason cannot be empty.");
        Guard.Against(value.Length > 1024, "ArchiveFailureReason cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
