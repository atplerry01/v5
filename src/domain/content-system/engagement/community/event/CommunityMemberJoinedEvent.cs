using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Community;

public sealed record CommunityMemberJoinedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CommunityId CommunityId, string MemberRef, CommunityRole Role, Timestamp JoinedAt) : DomainEvent;
