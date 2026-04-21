using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public readonly record struct EventSchema
{
    public string EventName { get; }
    public int SchemaVersion { get; }

    public EventSchema(string eventName, int schemaVersion)
    {
        Guard.Against(string.IsNullOrWhiteSpace(eventName), "Event name must not be empty.");
        Guard.Against(schemaVersion <= 0, "Schema version must be positive.");

        EventName = eventName;
        SchemaVersion = schemaVersion;
    }
}
