using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventSchema;

public sealed class EventSchemaAggregate : AggregateRoot
{
    public EventSchemaId EventSchemaId { get; private set; }
    public EventSchemaName Name { get; private set; }
    public new EventSchemaVersion Version { get; private set; }
    public CompatibilityMode CompatibilityMode { get; private set; }
    public bool IsDeprecated { get; private set; }

    private EventSchemaAggregate() { }

    public static EventSchemaAggregate Register(
        EventSchemaId id,
        EventSchemaName name,
        EventSchemaVersion version,
        CompatibilityMode compatibilityMode,
        Timestamp registeredAt)
    {
        var aggregate = new EventSchemaAggregate();
        if (((AggregateRoot)aggregate).Version >= 0)
            throw EventSchemaErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new EventSchemaRegisteredEvent(
            id, name, version, compatibilityMode, registeredAt));

        return aggregate;
    }

    public void Deprecate(Timestamp deprecatedAt)
    {
        if (IsDeprecated)
            throw EventSchemaErrors.AlreadyDeprecated();

        RaiseDomainEvent(new EventSchemaDeprecatedEvent(EventSchemaId, deprecatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EventSchemaRegisteredEvent e:
                EventSchemaId = e.EventSchemaId;
                Name = e.Name;
                Version = e.Version;
                CompatibilityMode = e.CompatibilityMode;
                IsDeprecated = false;
                break;

            case EventSchemaDeprecatedEvent:
                IsDeprecated = true;
                break;
        }
    }
}
