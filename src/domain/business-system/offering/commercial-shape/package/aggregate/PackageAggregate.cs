namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public sealed class PackageAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly HashSet<PackageMember> _members = new();

    public PackageId Id { get; private set; }
    public PackageCode Code { get; private set; }
    public PackageName Name { get; private set; }
    public PackageStatus Status { get; private set; }
    public IReadOnlyCollection<PackageMember> Members => _members;
    public int Version { get; private set; }

    private PackageAggregate() { }

    public static PackageAggregate Create(PackageId id, PackageCode code, PackageName name)
    {
        var aggregate = new PackageAggregate();

        var @event = new PackageCreatedEvent(id, code, name);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddMember(PackageMember member)
    {
        EnsureMutable();

        if (_members.Contains(member))
            throw PackageErrors.MemberAlreadyPresent(member);

        var @event = new PackageMemberAddedEvent(Id, member);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void RemoveMember(PackageMember member)
    {
        EnsureMutable();

        if (!_members.Contains(member))
            throw PackageErrors.MemberNotPresent(member);

        var @event = new PackageMemberRemovedEvent(Id, member);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status, _members.Count))
        {
            if (Status != PackageStatus.Draft)
                throw PackageErrors.InvalidStateTransition(Status, nameof(Activate));
            throw PackageErrors.ActivationRequiresMembers();
        }

        var @event = new PackageActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == PackageStatus.Archived)
            throw PackageErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new PackageArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(PackageCreatedEvent @event)
    {
        Id = @event.PackageId;
        Code = @event.Code;
        Name = @event.Name;
        Status = PackageStatus.Draft;
        Version++;
    }

    private void Apply(PackageMemberAddedEvent @event)
    {
        _members.Add(@event.Member);
        Version++;
    }

    private void Apply(PackageMemberRemovedEvent @event)
    {
        _members.Remove(@event.Member);
        Version++;
    }

    private void Apply(PackageActivatedEvent @event)
    {
        Status = PackageStatus.Active;
        Version++;
    }

    private void Apply(PackageArchivedEvent @event)
    {
        Status = PackageStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PackageErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw PackageErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw PackageErrors.InvalidStateTransition(Status, "validate");
    }
}
