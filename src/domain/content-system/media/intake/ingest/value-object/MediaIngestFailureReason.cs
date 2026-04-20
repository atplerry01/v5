using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public readonly record struct MediaIngestFailureReason
{
    public string Value { get; }

    public MediaIngestFailureReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MediaIngestFailureReason cannot be empty.");
        Guard.Against(value.Length > 1024, "MediaIngestFailureReason cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
