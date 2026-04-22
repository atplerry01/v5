using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventDefinition;

public readonly record struct EventVersion
{
    public string Value { get; }

    public EventVersion(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "EventVersion cannot be empty.");
        Value = value;
    }
}
