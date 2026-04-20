using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

public readonly record struct RetentionReason
{
    public string Value { get; }

    public RetentionReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "RetentionReason cannot be empty.");
        Guard.Against(value.Length > 1024, "RetentionReason cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
