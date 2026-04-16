using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Community;

public sealed record CommunityArchivedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CommunityId CommunityId, Timestamp ArchivedAt) : DomainEvent;
