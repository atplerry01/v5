namespace Whycespace.Domain.BusinessSystem.Integration.CommandBridge;

public sealed class CommandBridgeAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CommandBridgeId Id { get; private set; }
    public CommandMappingId MappingId { get; private set; }
    public CommandBridgeStatus Status { get; private set; }
    public int Version { get; private set; }

    private CommandBridgeAggregate() { }

    public static CommandBridgeAggregate Create(CommandBridgeId id, CommandMappingId mappingId)
    {
        var aggregate = new CommandBridgeAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new CommandBridgeCreatedEvent(id, mappingId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandBridgeErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new CommandBridgeActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Disable()
    {
        ValidateBeforeChange();

        var specification = new CanDisableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandBridgeErrors.InvalidStateTransition(Status, nameof(Disable));

        var @event = new CommandBridgeDisabledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(CommandBridgeCreatedEvent @event)
    {
        Id = @event.CommandBridgeId;
        MappingId = @event.MappingId;
        Status = CommandBridgeStatus.Defined;
        Version++;
    }

    private void Apply(CommandBridgeActivatedEvent @event)
    {
        Status = CommandBridgeStatus.Active;
        Version++;
    }

    private void Apply(CommandBridgeDisabledEvent @event)
    {
        Status = CommandBridgeStatus.Disabled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw CommandBridgeErrors.MissingId();

        if (MappingId == default)
            throw CommandBridgeErrors.MissingMappingId();

        if (!Enum.IsDefined(Status))
            throw CommandBridgeErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
