using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public sealed class AccountAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AccountId Id { get; private set; }
    public CustomerRef Customer { get; private set; }
    public AccountName Name { get; private set; }
    public AccountType Type { get; private set; }
    public AccountStatus Status { get; private set; }
    public int Version { get; private set; }

    private AccountAggregate() { }

    public static AccountAggregate Create(
        AccountId id,
        CustomerRef customer,
        AccountName name,
        AccountType type)
    {
        var aggregate = new AccountAggregate();

        var @event = new AccountCreatedEvent(id, customer, name, type);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Rename(AccountName name)
    {
        EnsureNotClosed();

        var @event = new AccountRenamedEvent(Id, name);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AccountErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new AccountActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AccountErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new AccountSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Close()
    {
        var specification = new CanCloseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AccountErrors.InvalidStateTransition(Status, nameof(Close));

        var @event = new AccountClosedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AccountCreatedEvent @event)
    {
        Id = @event.AccountId;
        Customer = @event.Customer;
        Name = @event.Name;
        Type = @event.Type;
        Status = AccountStatus.Draft;
        Version++;
    }

    private void Apply(AccountRenamedEvent @event)
    {
        Name = @event.Name;
        Version++;
    }

    private void Apply(AccountActivatedEvent @event)
    {
        Status = AccountStatus.Active;
        Version++;
    }

    private void Apply(AccountSuspendedEvent @event)
    {
        Status = AccountStatus.Suspended;
        Version++;
    }

    private void Apply(AccountClosedEvent @event)
    {
        Status = AccountStatus.Closed;
        Version++;
    }

    private void EnsureNotClosed()
    {
        if (Status == AccountStatus.Closed)
            throw AccountErrors.ClosedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw AccountErrors.MissingId();

        if (Customer == default)
            throw AccountErrors.MissingCustomerRef();

        if (!Enum.IsDefined(Status))
            throw AccountErrors.InvalidStateTransition(Status, "validate");
    }
}
