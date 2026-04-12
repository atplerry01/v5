namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public readonly record struct EventDefinitionId
{
    public Guid Value { get; }

    public EventDefinitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EventDefinitionId cannot be empty.", nameof(value));

        Value = value;
    }
}
