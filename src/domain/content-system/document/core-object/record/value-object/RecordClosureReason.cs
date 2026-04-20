using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Record;

public readonly record struct RecordClosureReason
{
    public string Value { get; }

    public RecordClosureReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "RecordClosureReason cannot be empty.");
        Guard.Against(value.Length > 1024, "RecordClosureReason cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
