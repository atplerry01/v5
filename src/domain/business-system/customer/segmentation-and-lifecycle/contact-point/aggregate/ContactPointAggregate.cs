using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed class ContactPointAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ContactPointId Id { get; private set; }
    public CustomerRef Customer { get; private set; }
    public ContactPointKind Kind { get; private set; }
    public ContactPointValue Value { get; private set; }
    public ContactPointStatus Status { get; private set; }
    public bool IsPreferred { get; private set; }
    public int Version { get; private set; }

    private ContactPointAggregate() { }

    public static ContactPointAggregate Create(
        ContactPointId id,
        CustomerRef customer,
        ContactPointKind kind,
        ContactPointValue value)
    {
        var aggregate = new ContactPointAggregate();

        var @event = new ContactPointCreatedEvent(id, customer, kind, value);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(ContactPointValue value)
    {
        EnsureMutable();

        var @event = new ContactPointUpdatedEvent(Id, value);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContactPointErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ContactPointActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void SetPreferred(bool isPreferred)
    {
        EnsureMutable();

        if (IsPreferred == isPreferred) return;

        var @event = new ContactPointPreferredSetEvent(Id, isPreferred);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ContactPointStatus.Archived)
            throw ContactPointErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ContactPointArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ContactPointCreatedEvent @event)
    {
        Id = @event.ContactPointId;
        Customer = @event.Customer;
        Kind = @event.Kind;
        Value = @event.Value;
        Status = ContactPointStatus.Draft;
        IsPreferred = false;
        Version++;
    }

    private void Apply(ContactPointUpdatedEvent @event)
    {
        Value = @event.Value;
        Version++;
    }

    private void Apply(ContactPointActivatedEvent @event)
    {
        Status = ContactPointStatus.Active;
        Version++;
    }

    private void Apply(ContactPointPreferredSetEvent @event)
    {
        IsPreferred = @event.IsPreferred;
        Version++;
    }

    private void Apply(ContactPointArchivedEvent @event)
    {
        Status = ContactPointStatus.Archived;
        IsPreferred = false;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContactPointErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ContactPointErrors.MissingId();

        if (Customer == default)
            throw ContactPointErrors.MissingCustomerRef();

        if (!Enum.IsDefined(Status))
            throw ContactPointErrors.InvalidStateTransition(Status, "validate");
    }
}
