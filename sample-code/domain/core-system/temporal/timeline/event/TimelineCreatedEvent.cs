using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Timeline;

public sealed record TimelineCreatedEvent(Guid TimelineId, string TimelineName, DateTimeOffset StartDate) : DomainEvent;
public sealed record TimelineExtendedEvent(Guid TimelineId, DateTimeOffset NewEndDate) : DomainEvent;
