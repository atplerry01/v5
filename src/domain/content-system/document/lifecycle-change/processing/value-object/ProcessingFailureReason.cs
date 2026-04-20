using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

public readonly record struct ProcessingFailureReason
{
    public string Value { get; }

    public ProcessingFailureReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ProcessingFailureReason cannot be empty.");
        Guard.Against(value.Length > 1024, "ProcessingFailureReason cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
