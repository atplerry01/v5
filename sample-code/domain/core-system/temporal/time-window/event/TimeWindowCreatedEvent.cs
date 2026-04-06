using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.TimeWindow;

public sealed record TimeWindowCreatedEvent(Guid WindowId, string WindowName, int DurationSeconds) : DomainEvent;
public sealed record TimeWindowClosedEvent(Guid WindowId) : DomainEvent;
