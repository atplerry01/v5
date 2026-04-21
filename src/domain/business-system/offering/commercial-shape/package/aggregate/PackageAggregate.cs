using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public sealed class PackageAggregate : AggregateRoot
{
    private readonly HashSet<PackageMember> _members = new();

    public PackageId Id { get; private set; }
    public PackageCode Code { get; private set; }
    public PackageName Name { get; private set; }
    public PackageStatus Status { get; private set; }
    public IReadOnlyCollection<PackageMember> Members => _members;

    public static PackageAggregate Create(PackageId id, PackageCode code, PackageName name)
    {
        var aggregate = new PackageAggregate();
        if (aggregate.Version >= 0)
            throw PackageErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new PackageCreatedEvent(id, code, name));
        return aggregate;
    }

    public void AddMember(PackageMember member)
    {
        EnsureMutable();

        if (_members.Contains(member))
            throw PackageErrors.MemberAlreadyPresent(member);

        RaiseDomainEvent(new PackageMemberAddedEvent(Id, member));
    }

    public void RemoveMember(PackageMember member)
    {
        EnsureMutable();

        if (!_members.Contains(member))
            throw PackageErrors.MemberNotPresent(member);

        RaiseDomainEvent(new PackageMemberRemovedEvent(Id, member));
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

        RaiseDomainEvent(new PackageActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == PackageStatus.Archived)
            throw PackageErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new PackageArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PackageCreatedEvent e:
                Id = e.PackageId;
                Code = e.Code;
                Name = e.Name;
                Status = PackageStatus.Draft;
                break;
            case PackageMemberAddedEvent e:
                _members.Add(e.Member);
                break;
            case PackageMemberRemovedEvent e:
                _members.Remove(e.Member);
                break;
            case PackageActivatedEvent:
                Status = PackageStatus.Active;
                break;
            case PackageArchivedEvent:
                Status = PackageStatus.Archived;
                break;
        }
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PackageErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw PackageErrors.MissingId();

        if (Code == default)
            throw PackageErrors.MissingCode();

        if (Name == default)
            throw PackageErrors.MissingName();

        if (!Enum.IsDefined(Status))
            throw PackageErrors.InvalidStateTransition(Status, "validate");
    }
}
