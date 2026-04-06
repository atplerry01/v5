using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Ordering;

public sealed record TemporalOrderingInitializedEvent(Guid OrderingId, string GuaranteeLevel, DateTimeOffset StartTime) : DomainEvent;
public sealed record SequenceAdvancedEvent(Guid OrderingId, long NewSequenceNumber, DateTimeOffset Timestamp) : DomainEvent;
public sealed record SchedulingConstraintRegisteredEvent(Guid OrderingId, DateTimeOffset NotBefore, DateTimeOffset NotAfter, bool RequiresMonotonic) : DomainEvent;
public sealed record OrderingViolationDetectedEvent(Guid OrderingId, DateTimeOffset ExpectedAfter, DateTimeOffset Actual) : DomainEvent;
public sealed record TemporalOrderingSealedEvent(Guid OrderingId, long FinalSequenceNumber) : DomainEvent;
