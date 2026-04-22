namespace Whycespace.Domain.PlatformSystem.Event.EventDefinition;

public readonly record struct EventDefinitionStatus
{
    public static readonly EventDefinitionStatus Active = new("Active");
    public static readonly EventDefinitionStatus Deprecated = new("Deprecated");

    public string Value { get; }

    private EventDefinitionStatus(string value) => Value = value;
}
