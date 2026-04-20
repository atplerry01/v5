namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public sealed class BundleAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly HashSet<BundleMember> _members = new();

    public BundleId Id { get; private set; }
    public BundleName Name { get; private set; }
    public BundleStatus Status { get; private set; }
    public IReadOnlyCollection<BundleMember> Members => _members;
    public int Version { get; private set; }

    private BundleAggregate() { }

    public static BundleAggregate Create(BundleId id, BundleName name)
    {
        var aggregate = new BundleAggregate();

        var @event = new BundleCreatedEvent(id, name);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddMember(BundleMember member)
    {
        EnsureMutable();

        if (_members.Contains(member))
            throw BundleErrors.MemberAlreadyPresent(member);

        var @event = new BundleMemberAddedEvent(Id, member);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void RemoveMember(BundleMember member)
    {
        EnsureMutable();

        if (!_members.Contains(member))
            throw BundleErrors.MemberNotPresent(member);

        var @event = new BundleMemberRemovedEvent(Id, member);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status, _members.Count))
        {
            if (Status != BundleStatus.Draft)
                throw BundleErrors.InvalidStateTransition(Status, nameof(Activate));

            throw BundleErrors.ActivationRequiresMembers();
        }

        var @event = new BundleActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == BundleStatus.Archived)
            throw BundleErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new BundleArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(BundleCreatedEvent @event)
    {
        Id = @event.BundleId;
        Name = @event.Name;
        Status = BundleStatus.Draft;
        Version++;
    }

    private void Apply(BundleMemberAddedEvent @event)
    {
        _members.Add(@event.Member);
        Version++;
    }

    private void Apply(BundleMemberRemovedEvent @event)
    {
        _members.Remove(@event.Member);
        Version++;
    }

    private void Apply(BundleActivatedEvent @event)
    {
        Status = BundleStatus.Active;
        Version++;
    }

    private void Apply(BundleArchivedEvent @event)
    {
        Status = BundleStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw BundleErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw BundleErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw BundleErrors.InvalidStateTransition(Status, "validate");
    }
}
