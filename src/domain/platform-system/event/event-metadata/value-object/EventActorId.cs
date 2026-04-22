using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventMetadata;

public readonly record struct EventActorId
{
    public string Value { get; }

    public EventActorId(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "EventActorId cannot be empty.");
        Value = value;
    }
}
