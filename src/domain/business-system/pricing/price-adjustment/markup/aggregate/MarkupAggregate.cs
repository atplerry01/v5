using Whycespace.Domain.BusinessSystem.Shared.Pricing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public sealed class MarkupAggregate : AggregateRoot
{
    public MarkupId Id { get; private set; }
    public MarkupCode Code { get; private set; }
    public MarkupName Name { get; private set; }
    public AdjustmentBasis Basis { get; private set; }
    public MarkupAmount Amount { get; private set; }
    public MarkupStatus Status { get; private set; }

    public static MarkupAggregate Create(
        MarkupId id,
        MarkupCode code,
        MarkupName name,
        AdjustmentBasis basis,
        MarkupAmount amount)
    {
        ValidateBasisAmount(basis, amount);

        var aggregate = new MarkupAggregate();
        if (aggregate.Version >= 0)
            throw MarkupErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new MarkupCreatedEvent(id, code, name, basis, amount));
        return aggregate;
    }

    public void Update(MarkupName name, AdjustmentBasis basis, MarkupAmount amount)
    {
        if (Status == MarkupStatus.Archived)
            throw MarkupErrors.InvalidStateTransition(Status, nameof(Update));

        ValidateBasisAmount(basis, amount);

        RaiseDomainEvent(new MarkupUpdatedEvent(Id, name, basis, amount));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MarkupErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new MarkupActivatedEvent(Id));
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MarkupErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new MarkupDeprecatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == MarkupStatus.Archived)
            throw MarkupErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new MarkupArchivedEvent(Id));
    }

    private static void ValidateBasisAmount(AdjustmentBasis basis, MarkupAmount amount)
    {
        if (basis == AdjustmentBasis.Percentage && (amount.Value < 0m || amount.Value > 100m))
            throw MarkupErrors.PercentageOutOfRange(amount.Value);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MarkupCreatedEvent e:
                Id = e.MarkupId;
                Code = e.Code;
                Name = e.Name;
                Basis = e.Basis;
                Amount = e.Amount;
                Status = MarkupStatus.Draft;
                break;
            case MarkupUpdatedEvent e:
                Name = e.Name;
                Basis = e.Basis;
                Amount = e.Amount;
                break;
            case MarkupActivatedEvent:
                Status = MarkupStatus.Active;
                break;
            case MarkupDeprecatedEvent:
                Status = MarkupStatus.Deprecated;
                break;
            case MarkupArchivedEvent:
                Status = MarkupStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw MarkupErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw MarkupErrors.InvalidStateTransition(Status, "validate");

        if (!Enum.IsDefined(Basis))
            throw MarkupErrors.InvalidStateTransition(Status, "validate-basis");
    }
}
