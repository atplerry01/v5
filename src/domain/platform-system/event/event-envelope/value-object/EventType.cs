using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventEnvelope;

public readonly record struct EventType
{
    public string Value { get; }

    public EventType(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "EventType cannot be empty.");
        Value = value;
    }
}
