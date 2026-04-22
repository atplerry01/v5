using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventDefinition;

public sealed class EventDefinitionAggregate : AggregateRoot
{
    public EventDefinitionId EventDefinitionId { get; private set; }
    public EventTypeName TypeName { get; private set; }
    public new EventVersion Version { get; private set; }
    public string SchemaId { get; private set; } = string.Empty;
    public DomainRoute SourceRoute { get; private set; } = null!;
    public EventDefinitionStatus Status { get; private set; }

    private EventDefinitionAggregate() { }

    public static EventDefinitionAggregate Define(
        EventDefinitionId id,
        EventTypeName typeName,
        EventVersion version,
        string schemaId,
        DomainRoute sourceRoute,
        Timestamp definedAt)
    {
        var aggregate = new EventDefinitionAggregate();
        if (((AggregateRoot)aggregate).Version >= 0)
            throw EventDefinitionErrors.AlreadyInitialized();

        if (string.IsNullOrWhiteSpace(schemaId))
            throw EventDefinitionErrors.SchemaIdMissing();

        if (!sourceRoute.IsValid())
            throw EventDefinitionErrors.SourceRouteMissing();

        aggregate.RaiseDomainEvent(new EventDefinedEvent(
            id, typeName, version, schemaId, sourceRoute, definedAt));

        return aggregate;
    }

    public void Deprecate(Timestamp deprecatedAt)
    {
        if (Status == EventDefinitionStatus.Deprecated)
            throw EventDefinitionErrors.AlreadyDeprecated();

        RaiseDomainEvent(new EventDeprecatedEvent(EventDefinitionId, deprecatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EventDefinedEvent e:
                EventDefinitionId = e.EventDefinitionId;
                TypeName = e.TypeName;
                Version = e.Version;
                SchemaId = e.SchemaId;
                SourceRoute = e.SourceRoute;
                Status = EventDefinitionStatus.Active;
                break;

            case EventDeprecatedEvent:
                Status = EventDefinitionStatus.Deprecated;
                break;
        }
    }
}
