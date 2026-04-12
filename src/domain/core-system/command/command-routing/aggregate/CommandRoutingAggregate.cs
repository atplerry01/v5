namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public sealed class CommandRoutingAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CommandRoutingId Id { get; private set; }
    public RoutingRule Rule { get; private set; }
    public CommandRoutingStatus Status { get; private set; }
    public int Version { get; private set; }

    private CommandRoutingAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static CommandRoutingAggregate Define(
        CommandRoutingId id,
        RoutingRule rule)
    {
        var aggregate = new CommandRoutingAggregate();

        var @event = new CommandRoutingDefinedEvent(id, rule);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // ── Activate ─────────────────────────────────────────────────

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandRoutingErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new CommandRoutingActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Disable ──────────────────────────────────────────────────

    public void Disable()
    {
        var specification = new CanDisableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandRoutingErrors.InvalidStateTransition(Status, nameof(Disable));

        var @event = new CommandRoutingDisabledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Apply ────────────────────────────────────────────────────

    private void Apply(CommandRoutingDefinedEvent @event)
    {
        Id = @event.RoutingId;
        Rule = @event.Rule;
        Status = CommandRoutingStatus.Defined;
        Version++;
    }

    private void Apply(CommandRoutingActivatedEvent @event)
    {
        Status = CommandRoutingStatus.Active;
        Version++;
    }

    private void Apply(CommandRoutingDisabledEvent @event)
    {
        Status = CommandRoutingStatus.Disabled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw CommandRoutingErrors.MissingId();

        if (Rule == default)
            throw CommandRoutingErrors.MissingRoutingRule();

        if (!Enum.IsDefined(Status))
            throw CommandRoutingErrors.InvalidStateTransition(Status, "validate");
    }
}
