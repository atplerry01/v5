using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;

public sealed class ClauseAggregate : AggregateRoot
{
    public ClauseId Id { get; private set; }
    public ClauseType Type { get; private set; }
    public ClauseStatus Status { get; private set; }

    public static ClauseAggregate Create(ClauseId id, ClauseType clauseType)
    {
        var aggregate = new ClauseAggregate();
        if (aggregate.Version >= 0)
            throw ClauseErrors.AlreadyInitialized();

        var validationSpec = new IsValidClauseSpecification();
        if (!validationSpec.IsSatisfiedBy(id, clauseType))
            throw ClauseErrors.InvalidClauseType();

        aggregate.RaiseDomainEvent(new ClauseCreatedEvent(id, clauseType));
        return aggregate;
    }

    public void Activate()
    {
        var specification = new CanActivateClauseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ClauseErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ClauseActivatedEvent(Id));
    }

    public void Supersede()
    {
        var specification = new CanSupersedeClauseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ClauseErrors.InvalidStateTransition(Status, nameof(Supersede));

        RaiseDomainEvent(new ClauseSupersededEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ClauseCreatedEvent e:
                Id = e.ClauseId;
                Type = e.ClauseType;
                Status = ClauseStatus.Draft;
                break;
            case ClauseActivatedEvent:
                Status = ClauseStatus.Active;
                break;
            case ClauseSupersededEvent:
                Status = ClauseStatus.Superseded;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ClauseErrors.MissingId();

        if (!Enum.IsDefined(Type))
            throw ClauseErrors.InvalidClauseType();

        if (!Enum.IsDefined(Status))
            throw ClauseErrors.InvalidStateTransition(Status, "validate");
    }
}
