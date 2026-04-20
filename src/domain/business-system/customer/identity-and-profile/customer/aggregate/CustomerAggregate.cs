namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public sealed class CustomerAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CustomerId Id { get; private set; }
    public CustomerName Name { get; private set; }
    public CustomerType Type { get; private set; }
    public CustomerStatus Status { get; private set; }
    public CustomerReferenceCode? ReferenceCode { get; private set; }
    public int Version { get; private set; }

    private CustomerAggregate() { }

    public static CustomerAggregate Create(
        CustomerId id,
        CustomerName name,
        CustomerType type,
        CustomerReferenceCode? referenceCode = null)
    {
        var aggregate = new CustomerAggregate();

        var @event = new CustomerCreatedEvent(id, name, type, referenceCode);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Rename(CustomerName name)
    {
        EnsureMutable(nameof(Rename));

        var @event = new CustomerRenamedEvent(Id, name);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reclassify(CustomerType type)
    {
        EnsureMutable(nameof(Reclassify));

        var @event = new CustomerReclassifiedEvent(Id, type);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CustomerErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new CustomerActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == CustomerStatus.Archived)
            throw CustomerErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new CustomerArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(CustomerCreatedEvent @event)
    {
        Id = @event.CustomerId;
        Name = @event.Name;
        Type = @event.Type;
        ReferenceCode = @event.ReferenceCode;
        Status = CustomerStatus.Draft;
        Version++;
    }

    private void Apply(CustomerRenamedEvent @event)
    {
        Name = @event.Name;
        Version++;
    }

    private void Apply(CustomerReclassifiedEvent @event)
    {
        Type = @event.Type;
        Version++;
    }

    private void Apply(CustomerActivatedEvent @event)
    {
        Status = CustomerStatus.Active;
        Version++;
    }

    private void Apply(CustomerArchivedEvent @event)
    {
        Status = CustomerStatus.Archived;
        Version++;
    }

    private void EnsureMutable(string attemptedAction)
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CustomerErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw CustomerErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw CustomerErrors.InvalidStateTransition(Status, "validate");

        if (!Enum.IsDefined(Type))
            throw CustomerErrors.InvalidStateTransition(Status, "validate-type");
    }
}
