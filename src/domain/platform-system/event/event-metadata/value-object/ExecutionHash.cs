using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventMetadata;

public readonly record struct ExecutionHash
{
    public const string ReplaySentinel = "replay";

    public string Value { get; }

    public ExecutionHash(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ExecutionHash cannot be empty.");
        Value = value;
    }

    public bool IsReplay => Value == ReplaySentinel;
}
