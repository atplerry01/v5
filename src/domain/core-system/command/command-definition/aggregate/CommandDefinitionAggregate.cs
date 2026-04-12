namespace Whycespace.Domain.CoreSystem.Command.CommandDefinition;

public sealed class CommandDefinitionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CommandDefinitionId Id { get; private set; }
    public CommandSchema Schema { get; private set; }
    public CommandDefinitionStatus Status { get; private set; }
    public int Version { get; private set; }

    private CommandDefinitionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static CommandDefinitionAggregate Register(
        CommandDefinitionId id,
        CommandSchema schema)
    {
        var aggregate = new CommandDefinitionAggregate();

        var @event = new CommandDefinitionRegisteredEvent(id, schema);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // ── Publish ──────────────────────────────────────────────────

    public void Publish()
    {
        var specification = new CanPublishSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandDefinitionErrors.InvalidStateTransition(Status, nameof(Publish));

        var @event = new CommandDefinitionPublishedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Deprecate ────────────────────────────────────────────────

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandDefinitionErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new CommandDefinitionDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Apply ────────────────────────────────────────────────────

    private void Apply(CommandDefinitionRegisteredEvent @event)
    {
        Id = @event.DefinitionId;
        Schema = @event.Schema;
        Status = CommandDefinitionStatus.Draft;
        Version++;
    }

    private void Apply(CommandDefinitionPublishedEvent @event)
    {
        Status = CommandDefinitionStatus.Published;
        Version++;
    }

    private void Apply(CommandDefinitionDeprecatedEvent @event)
    {
        Status = CommandDefinitionStatus.Deprecated;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw CommandDefinitionErrors.MissingId();

        if (Schema == default)
            throw CommandDefinitionErrors.MissingSchema();

        if (!Enum.IsDefined(Status))
            throw CommandDefinitionErrors.InvalidStateTransition(Status, "validate");
    }
}
