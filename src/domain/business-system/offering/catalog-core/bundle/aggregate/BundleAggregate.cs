using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public sealed class BundleAggregate : AggregateRoot
{
    private readonly HashSet<BundleMember> _members = new();

    public BundleId Id { get; private set; }
    public BundleName Name { get; private set; }
    public BundleStatus Status { get; private set; }
    public IReadOnlyCollection<BundleMember> Members => _members;

    public static BundleAggregate Create(BundleId id, BundleName name)
    {
        var aggregate = new BundleAggregate();
        if (aggregate.Version >= 0)
            throw BundleErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new BundleCreatedEvent(id, name));
        return aggregate;
    }

    public void AddMember(BundleMember member)
    {
        EnsureMutable();

        if (_members.Contains(member))
            throw BundleErrors.MemberAlreadyPresent(member);

        RaiseDomainEvent(new BundleMemberAddedEvent(Id, member));
    }

    public void RemoveMember(BundleMember member)
    {
        EnsureMutable();

        if (!_members.Contains(member))
            throw BundleErrors.MemberNotPresent(member);

        RaiseDomainEvent(new BundleMemberRemovedEvent(Id, member));
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

        RaiseDomainEvent(new BundleActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == BundleStatus.Archived)
            throw BundleErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new BundleArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case BundleCreatedEvent e:
                Id = e.BundleId;
                Name = e.Name;
                Status = BundleStatus.Draft;
                break;
            case BundleMemberAddedEvent e:
                _members.Add(e.Member);
                break;
            case BundleMemberRemovedEvent e:
                _members.Remove(e.Member);
                break;
            case BundleActivatedEvent:
                Status = BundleStatus.Active;
                break;
            case BundleArchivedEvent:
                Status = BundleStatus.Archived;
                break;
        }
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw BundleErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw BundleErrors.MissingId();

        if (Name == default)
            throw BundleErrors.MissingName();

        if (!Enum.IsDefined(Status))
            throw BundleErrors.InvalidStateTransition(Status, "validate");
    }
}
