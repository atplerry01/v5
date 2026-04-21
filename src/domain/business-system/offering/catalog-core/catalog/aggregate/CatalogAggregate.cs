using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public sealed class CatalogAggregate : AggregateRoot
{
    private readonly HashSet<CatalogMember> _members = new();

    public CatalogId Id { get; private set; }
    public CatalogStatus Status { get; private set; }
    public CatalogStructure Structure { get; private set; }
    public IReadOnlyCollection<CatalogMember> Members => _members;

    public static CatalogAggregate Create(CatalogId id, CatalogStructure structure)
    {
        var aggregate = new CatalogAggregate();
        if (aggregate.Version >= 0)
            throw CatalogErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new CatalogCreatedEvent(id, structure));
        return aggregate;
    }

    public void Publish()
    {
        var specification = new CanPublishSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CatalogErrors.InvalidStateTransition(Status, nameof(Publish));

        RaiseDomainEvent(new CatalogPublishedEvent(Id));
    }

    public void Archive()
    {
        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CatalogErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new CatalogArchivedEvent(Id));
    }

    public void AddMember(CatalogMember member)
    {
        EnsureNotArchived();

        if (_members.Contains(member))
            throw CatalogErrors.MemberAlreadyPresent(member);

        RaiseDomainEvent(new CatalogMemberAddedEvent(Id, member));
    }

    public void RemoveMember(CatalogMember member)
    {
        EnsureNotArchived();

        if (!_members.Contains(member))
            throw CatalogErrors.MemberNotPresent(member);

        RaiseDomainEvent(new CatalogMemberRemovedEvent(Id, member));
    }

    private void EnsureNotArchived()
    {
        if (Status == CatalogStatus.Archived)
            throw CatalogErrors.ArchivedImmutable(Id);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CatalogCreatedEvent e:
                Id = e.CatalogId;
                Structure = e.Structure;
                Status = CatalogStatus.Draft;
                break;
            case CatalogPublishedEvent:
                Status = CatalogStatus.Published;
                break;
            case CatalogArchivedEvent:
                Status = CatalogStatus.Archived;
                break;
            case CatalogMemberAddedEvent e:
                _members.Add(e.Member);
                break;
            case CatalogMemberRemovedEvent e:
                _members.Remove(e.Member);
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw CatalogErrors.MissingId();

        if (Structure == default)
            throw CatalogErrors.MissingStructure();

        if (!Enum.IsDefined(Status))
            throw CatalogErrors.InvalidStateTransition(Status, "validate");
    }
}
