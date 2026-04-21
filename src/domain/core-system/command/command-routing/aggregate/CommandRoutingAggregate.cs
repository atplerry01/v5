using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public sealed class CommandRoutingAggregate : AggregateRoot
{
    public CommandRoutingId Id { get; private set; }
    public RoutingRule Rule { get; private set; }
    public CommandRoutingStatus Status { get; private set; }

    // ── Factory ──────────────────────────────────────────────────

    public static CommandRoutingAggregate Define(
        CommandRoutingId id,
        RoutingRule rule)
    {
        var aggregate = new CommandRoutingAggregate();
        if (aggregate.Version >= 0)
            throw CommandRoutingErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new CommandRoutingDefinedEvent(id, rule));
        return aggregate;
    }

    // ── Activate ─────────────────────────────────────────────────

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandRoutingErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new CommandRoutingActivatedEvent(Id));
    }

    // ── Disable ──────────────────────────────────────────────────

    public void Disable()
    {
        var specification = new CanDisableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandRoutingErrors.InvalidStateTransition(Status, nameof(Disable));

        RaiseDomainEvent(new CommandRoutingDisabledEvent(Id));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CommandRoutingDefinedEvent e:
                Id = e.RoutingId;
                Rule = e.Rule;
                Status = CommandRoutingStatus.Defined;
                break;
            case CommandRoutingActivatedEvent:
                Status = CommandRoutingStatus.Active;
                break;
            case CommandRoutingDisabledEvent:
                Status = CommandRoutingStatus.Disabled;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw CommandRoutingErrors.MissingId();

        if (Rule == default)
            throw CommandRoutingErrors.MissingRoutingRule();

        if (!Enum.IsDefined(Status))
            throw CommandRoutingErrors.InvalidStateTransition(Status, "validate");
    }
}
