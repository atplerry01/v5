using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventMetadata;

public readonly record struct PolicyDecisionHash
{
    public const string ReplaySentinel = "replay";

    public string Value { get; }

    public PolicyDecisionHash(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "PolicyDecisionHash cannot be empty.");
        Value = value;
    }

    public bool IsReplay => Value == ReplaySentinel;
}
