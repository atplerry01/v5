using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public readonly record struct SessionTerminationReason
{
    public string Value { get; }

    public SessionTerminationReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "SessionTerminationReason cannot be empty.");
        Guard.Against(value.Length > 1024, "SessionTerminationReason cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
