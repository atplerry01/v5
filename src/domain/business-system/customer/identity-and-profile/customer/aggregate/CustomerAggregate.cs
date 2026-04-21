using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public sealed class CustomerAggregate : AggregateRoot
{
    public CustomerId Id { get; private set; }
    public CustomerName Name { get; private set; }
    public CustomerType Type { get; private set; }
    public CustomerStatus Status { get; private set; }
    public CustomerReferenceCode? ReferenceCode { get; private set; }

    public static CustomerAggregate Create(
        CustomerId id,
        CustomerName name,
        CustomerType type,
        CustomerReferenceCode? referenceCode = null)
    {
        var aggregate = new CustomerAggregate();
        if (aggregate.Version >= 0)
            throw CustomerErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new CustomerCreatedEvent(id, name, type, referenceCode));
        return aggregate;
    }

    public void Rename(CustomerName name)
    {
        EnsureMutable(nameof(Rename));
        RaiseDomainEvent(new CustomerRenamedEvent(Id, name));
    }

    public void Reclassify(CustomerType type)
    {
        EnsureMutable(nameof(Reclassify));
        RaiseDomainEvent(new CustomerReclassifiedEvent(Id, type));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CustomerErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new CustomerActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == CustomerStatus.Archived)
            throw CustomerErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new CustomerArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CustomerCreatedEvent e:
                Id = e.CustomerId;
                Name = e.Name;
                Type = e.Type;
                ReferenceCode = e.ReferenceCode;
                Status = CustomerStatus.Draft;
                break;
            case CustomerRenamedEvent e:
                Name = e.Name;
                break;
            case CustomerReclassifiedEvent e:
                Type = e.Type;
                break;
            case CustomerActivatedEvent:
                Status = CustomerStatus.Active;
                break;
            case CustomerArchivedEvent:
                Status = CustomerStatus.Archived;
                break;
        }
    }

    private void EnsureMutable(string attemptedAction)
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CustomerErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw CustomerErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw CustomerErrors.InvalidStateTransition(Status, "validate");

        if (!Enum.IsDefined(Type))
            throw CustomerErrors.InvalidStateTransition(Status, "validate-type");
    }
}
