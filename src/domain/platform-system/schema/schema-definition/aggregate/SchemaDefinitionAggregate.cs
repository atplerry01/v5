using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

public sealed class SchemaDefinitionAggregate : AggregateRoot
{
    public SchemaDefinitionId SchemaDefinitionId { get; private set; }
    public SchemaName SchemaName { get; private set; }
    public int SchemaVersion { get; private set; }
    public IReadOnlyList<FieldDescriptor> Fields { get; private set; } = [];
    public SchemaCompatibilityMode CompatibilityMode { get; private set; }
    public SchemaStatus Status { get; private set; }

    private SchemaDefinitionAggregate() { }

    public static SchemaDefinitionAggregate Draft(
        SchemaDefinitionId id,
        SchemaName schemaName,
        int version,
        IReadOnlyList<FieldDescriptor> fields,
        SchemaCompatibilityMode compatibilityMode,
        Timestamp draftedAt)
    {
        var aggregate = new SchemaDefinitionAggregate();
        if (aggregate.Version >= 0)
            throw SchemaDefinitionErrors.AlreadyInitialized();

        if (fields is null || fields.Count == 0)
            throw SchemaDefinitionErrors.FieldsRequired();

        aggregate.RaiseDomainEvent(new SchemaDefinitionDraftedEvent(
            id, schemaName, version, fields, compatibilityMode, draftedAt));

        return aggregate;
    }

    public void Publish(Timestamp publishedAt)
    {
        if (Status == SchemaStatus.Deprecated)
            throw SchemaDefinitionErrors.AlreadyDeprecated();

        if (Status == SchemaStatus.Published)
            throw SchemaDefinitionErrors.AlreadyPublished();

        if (Status != SchemaStatus.Draft)
            throw SchemaDefinitionErrors.NotInDraftState();

        RaiseDomainEvent(new SchemaDefinitionPublishedEvent(SchemaDefinitionId, publishedAt));
    }

    public void Deprecate(Timestamp deprecatedAt)
    {
        if (Status == SchemaStatus.Deprecated)
            throw SchemaDefinitionErrors.AlreadyDeprecated();

        RaiseDomainEvent(new SchemaDefinitionDeprecatedEvent(SchemaDefinitionId, deprecatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SchemaDefinitionDraftedEvent e:
                SchemaDefinitionId = e.SchemaDefinitionId;
                SchemaName = e.SchemaName;
                SchemaVersion = e.Version;
                Fields = e.Fields;
                CompatibilityMode = e.CompatibilityMode;
                Status = SchemaStatus.Draft;
                break;

            case SchemaDefinitionPublishedEvent:
                Status = SchemaStatus.Published;
                break;

            case SchemaDefinitionDeprecatedEvent:
                Status = SchemaStatus.Deprecated;
                break;
        }
    }
}
