using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Community;

public sealed class CommunityAggregate : AggregateRoot
{
    private static readonly CommunitySpecification Spec = new();
    private readonly Dictionary<string, CommunityMember> _members = new(StringComparer.Ordinal);

    public CommunityId CommunityId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CommunityStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public IReadOnlyCollection<CommunityMember> Members => _members.Values;

    private CommunityAggregate() { }

    public static CommunityAggregate Create(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        CommunityId id, string name, string ownerRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(name)) throw CommunityErrors.InvalidName();
        if (string.IsNullOrWhiteSpace(ownerRef)) throw CommunityErrors.InvalidOwner();
        var agg = new CommunityAggregate();
        agg.RaiseDomainEvent(new CommunityCreatedEvent(eventId, aggregateId, correlationId, causationId, id, name.Trim(), ownerRef, at));
        agg.RaiseDomainEvent(new CommunityMemberJoinedEvent(eventId, aggregateId, correlationId, causationId, id, ownerRef, CommunityRole.Owner, at));
        return agg;
    }

    public void JoinMember(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string memberRef, CommunityRole role, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        if (string.IsNullOrWhiteSpace(memberRef)) throw CommunityErrors.InvalidMember();
        if (_members.TryGetValue(memberRef, out var m) && m.IsActive)
            throw CommunityErrors.MemberAlreadyJoined();
        RaiseDomainEvent(new CommunityMemberJoinedEvent(eventId, aggregateId, correlationId, causationId, CommunityId, memberRef, role, at));
    }

    public void LeaveMember(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string memberRef, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        if (!_members.TryGetValue(memberRef, out var m) || !m.IsActive)
            throw CommunityErrors.MemberNotInCommunity();
        RaiseDomainEvent(new CommunityMemberLeftEvent(eventId, aggregateId, correlationId, causationId, CommunityId, memberRef, at));
    }

    public void AssignRole(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string memberRef, CommunityRole role, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        if (!_members.TryGetValue(memberRef, out var m) || !m.IsActive)
            throw CommunityErrors.MemberNotInCommunity();
        RaiseDomainEvent(new CommunityRoleAssignedEvent(eventId, aggregateId, correlationId, causationId, CommunityId, memberRef, role, at));
    }

    public void Archive(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == CommunityStatus.Archived) throw CommunityErrors.AlreadyArchived();
        RaiseDomainEvent(new CommunityArchivedEvent(eventId, aggregateId, correlationId, causationId, CommunityId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CommunityCreatedEvent e:
                CommunityId = e.CommunityId;
                Name = e.Name;
                Status = CommunityStatus.Active;
                CreatedAt = e.CreatedAt;
                break;
            case CommunityMemberJoinedEvent e:
                _members[e.MemberRef] = CommunityMember.Join(e.MemberRef, e.Role, e.JoinedAt);
                break;
            case CommunityMemberLeftEvent e:
                if (_members.TryGetValue(e.MemberRef, out var m)) m.Leave(e.LeftAt);
                break;
            case CommunityRoleAssignedEvent e:
                if (_members.TryGetValue(e.MemberRef, out var r)) r.AssignRole(e.Role);
                break;
            case CommunityArchivedEvent: Status = CommunityStatus.Archived; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Status != CommunityStatus.Active) return;
        if (_members.Count == 0) return;
        var hasOwner = _members.Values.Any(m => m.IsActive && m.Role == CommunityRole.Owner);
        if (!hasOwner) throw CommunityErrors.OwnerRequired();
    }
}
