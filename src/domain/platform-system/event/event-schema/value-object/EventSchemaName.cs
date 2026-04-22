using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventSchema;

public readonly record struct EventSchemaName
{
    public string Value { get; }

    public EventSchemaName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "EventSchemaName cannot be empty.");
        Value = value;
    }
}
