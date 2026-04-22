using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandDefinition;

public sealed class CommandDefinitionAggregate : AggregateRoot
{
    public CommandDefinitionId CommandDefinitionId { get; private set; }
    public CommandTypeName TypeName { get; private set; }
    public CommandVersion TypeVersion { get; private set; }
    public string SchemaId { get; private set; } = string.Empty;
    public DomainRoute OwnerRoute { get; private set; } = null!;
    public CommandDefinitionStatus Status { get; private set; }

    private CommandDefinitionAggregate() { }

    public static CommandDefinitionAggregate Define(
        CommandDefinitionId id,
        CommandTypeName typeName,
        CommandVersion version,
        string schemaId,
        DomainRoute ownerRoute,
        Timestamp definedAt)
    {
        var aggregate = new CommandDefinitionAggregate();
        if (aggregate.Version >= 0)
            throw CommandDefinitionErrors.AlreadyInitialized();

        if (string.IsNullOrWhiteSpace(schemaId))
            throw CommandDefinitionErrors.MissingSchemaId();

        if (!ownerRoute.IsValid())
            throw CommandDefinitionErrors.InvalidOwnerRoute();

        aggregate.RaiseDomainEvent(new CommandDefinedEvent(id, typeName, version, schemaId, ownerRoute));
        return aggregate;
    }

    public void Deprecate(Timestamp deprecatedAt)
    {
        if (Status == CommandDefinitionStatus.Deprecated)
            throw CommandDefinitionErrors.AlreadyDeprecated();

        RaiseDomainEvent(new CommandDeprecatedEvent(CommandDefinitionId, TypeName, TypeVersion, deprecatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CommandDefinedEvent e:
                CommandDefinitionId = e.CommandDefinitionId;
                TypeName = e.TypeName;
                TypeVersion = e.Version;
                SchemaId = e.SchemaId;
                OwnerRoute = e.OwnerRoute;
                Status = CommandDefinitionStatus.Active;
                break;

            case CommandDeprecatedEvent:
                Status = CommandDefinitionStatus.Deprecated;
                break;
        }
    }
}
