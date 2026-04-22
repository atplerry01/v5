using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventDefinition;

public readonly record struct EventTypeName
{
    public string Value { get; }

    public EventTypeName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "EventTypeName cannot be empty.");
        Value = value;
    }
}
