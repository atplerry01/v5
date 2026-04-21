using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public sealed class StateTransitionAggregate : AggregateRoot
{
    public StateTransitionId Id { get; private set; }
    public TransitionRule Rule { get; private set; }
    public TransitionStatus Status { get; private set; }

    public static StateTransitionAggregate Define(
        StateTransitionId id,
        TransitionRule rule)
    {
        var aggregate = new StateTransitionAggregate();
        if (aggregate.Version >= 0)
            throw StateTransitionErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new StateTransitionDefinedEvent(id, rule));
        return aggregate;
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StateTransitionErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new StateTransitionActivatedEvent(Id));
    }

    public void Retire()
    {
        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StateTransitionErrors.InvalidStateTransition(Status, nameof(Retire));

        RaiseDomainEvent(new StateTransitionRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case StateTransitionDefinedEvent e:
                Id = e.TransitionId;
                Rule = e.Rule;
                Status = TransitionStatus.Defined;
                break;
            case StateTransitionActivatedEvent:
                Status = TransitionStatus.Active;
                break;
            case StateTransitionRetiredEvent:
                Status = TransitionStatus.Retired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw StateTransitionErrors.MissingId();

        if (Rule == default)
            throw StateTransitionErrors.MissingTransitionRule();

        if (!Enum.IsDefined(Status))
            throw StateTransitionErrors.InvalidStateTransition(Status, "validate");
    }
}
