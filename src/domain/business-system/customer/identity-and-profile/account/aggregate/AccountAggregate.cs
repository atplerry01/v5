using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public sealed class AccountAggregate : AggregateRoot
{
    public AccountId Id { get; private set; }
    public CustomerRef Customer { get; private set; }
    public AccountName Name { get; private set; }
    public AccountType Type { get; private set; }
    public AccountStatus Status { get; private set; }

    public static AccountAggregate Create(
        AccountId id,
        CustomerRef customer,
        AccountName name,
        AccountType type)
    {
        var aggregate = new AccountAggregate();
        if (aggregate.Version >= 0)
            throw AccountErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new AccountCreatedEvent(id, customer, name, type));
        return aggregate;
    }

    public void Rename(AccountName name)
    {
        EnsureNotClosed();
        RaiseDomainEvent(new AccountRenamedEvent(Id, name));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AccountErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new AccountActivatedEvent(Id));
    }

    public void Suspend()
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AccountErrors.InvalidStateTransition(Status, nameof(Suspend));

        RaiseDomainEvent(new AccountSuspendedEvent(Id));
    }

    public void Close()
    {
        var specification = new CanCloseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AccountErrors.InvalidStateTransition(Status, nameof(Close));

        RaiseDomainEvent(new AccountClosedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AccountCreatedEvent e:
                Id = e.AccountId;
                Customer = e.Customer;
                Name = e.Name;
                Type = e.Type;
                Status = AccountStatus.Draft;
                break;
            case AccountRenamedEvent e:
                Name = e.Name;
                break;
            case AccountActivatedEvent:
                Status = AccountStatus.Active;
                break;
            case AccountSuspendedEvent:
                Status = AccountStatus.Suspended;
                break;
            case AccountClosedEvent:
                Status = AccountStatus.Closed;
                break;
        }
    }

    private void EnsureNotClosed()
    {
        if (Status == AccountStatus.Closed)
            throw AccountErrors.ClosedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw AccountErrors.MissingId();

        if (Customer == default)
            throw AccountErrors.MissingCustomerRef();

        if (!Enum.IsDefined(Status))
            throw AccountErrors.InvalidStateTransition(Status, "validate");
    }
}
