namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public readonly record struct EventSchema
{
    public string EventName { get; }
    public int SchemaVersion { get; }

    public EventSchema(string eventName, int schemaVersion)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentException("Event name must not be empty.", nameof(eventName));

        if (schemaVersion <= 0)
            throw new ArgumentException("Schema version must be positive.", nameof(schemaVersion));

        EventName = eventName;
        SchemaVersion = schemaVersion;
    }
}
