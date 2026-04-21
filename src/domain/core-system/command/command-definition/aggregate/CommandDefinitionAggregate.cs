using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandDefinition;

public sealed class CommandDefinitionAggregate : AggregateRoot
{
    public CommandDefinitionId Id { get; private set; }
    public CommandSchema Schema { get; private set; }
    public CommandDefinitionStatus Status { get; private set; }

    // ── Factory ──────────────────────────────────────────────────

    public static CommandDefinitionAggregate Register(
        CommandDefinitionId id,
        CommandSchema schema)
    {
        var aggregate = new CommandDefinitionAggregate();
        if (aggregate.Version >= 0)
            throw CommandDefinitionErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new CommandDefinitionRegisteredEvent(id, schema));
        return aggregate;
    }

    // ── Publish ──────────────────────────────────────────────────

    public void Publish()
    {
        var specification = new CanPublishSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandDefinitionErrors.InvalidStateTransition(Status, nameof(Publish));

        RaiseDomainEvent(new CommandDefinitionPublishedEvent(Id));
    }

    // ── Deprecate ────────────────────────────────────────────────

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandDefinitionErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new CommandDefinitionDeprecatedEvent(Id));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CommandDefinitionRegisteredEvent e:
                Id = e.DefinitionId;
                Schema = e.Schema;
                Status = CommandDefinitionStatus.Draft;
                break;
            case CommandDefinitionPublishedEvent:
                Status = CommandDefinitionStatus.Published;
                break;
            case CommandDefinitionDeprecatedEvent:
                Status = CommandDefinitionStatus.Deprecated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw CommandDefinitionErrors.MissingId();

        if (Schema == default)
            throw CommandDefinitionErrors.MissingSchema();

        if (!Enum.IsDefined(Status))
            throw CommandDefinitionErrors.InvalidStateTransition(Status, "validate");
    }
}
