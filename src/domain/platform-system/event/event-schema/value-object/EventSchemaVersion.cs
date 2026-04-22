using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventSchema;

public readonly record struct EventSchemaVersion
{
    public string Value { get; }

    public EventSchemaVersion(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "EventSchemaVersion cannot be empty.");
        Value = value;
    }
}
