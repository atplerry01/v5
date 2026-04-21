using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed class ContactPointAggregate : AggregateRoot
{
    public ContactPointId Id { get; private set; }
    public CustomerRef Customer { get; private set; }
    public ContactPointKind Kind { get; private set; }
    public ContactPointValue Value { get; private set; }
    public ContactPointStatus Status { get; private set; }
    public bool IsPreferred { get; private set; }

    public static ContactPointAggregate Create(
        ContactPointId id,
        CustomerRef customer,
        ContactPointKind kind,
        ContactPointValue value)
    {
        var aggregate = new ContactPointAggregate();
        if (aggregate.Version >= 0)
            throw ContactPointErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ContactPointCreatedEvent(id, customer, kind, value));
        return aggregate;
    }

    public void Update(ContactPointValue value)
    {
        EnsureMutable();
        RaiseDomainEvent(new ContactPointUpdatedEvent(Id, value));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContactPointErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ContactPointActivatedEvent(Id));
    }

    public void SetPreferred(bool isPreferred)
    {
        EnsureMutable();

        if (IsPreferred == isPreferred) return;

        RaiseDomainEvent(new ContactPointPreferredSetEvent(Id, isPreferred));
    }

    public void Archive()
    {
        if (Status == ContactPointStatus.Archived)
            throw ContactPointErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ContactPointArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ContactPointCreatedEvent e:
                Id = e.ContactPointId;
                Customer = e.Customer;
                Kind = e.Kind;
                Value = e.Value;
                Status = ContactPointStatus.Draft;
                IsPreferred = false;
                break;
            case ContactPointUpdatedEvent e:
                Value = e.Value;
                break;
            case ContactPointActivatedEvent:
                Status = ContactPointStatus.Active;
                break;
            case ContactPointPreferredSetEvent e:
                IsPreferred = e.IsPreferred;
                break;
            case ContactPointArchivedEvent:
                Status = ContactPointStatus.Archived;
                IsPreferred = false;
                break;
        }
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContactPointErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ContactPointErrors.MissingId();

        if (Customer == default)
            throw ContactPointErrors.MissingCustomerRef();

        if (!Enum.IsDefined(Status))
            throw ContactPointErrors.InvalidStateTransition(Status, "validate");
    }
}
