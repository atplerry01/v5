using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.State.SystemState;

public sealed record SystemStateInitializedEvent(Guid StateId) : DomainEvent;
public sealed record SystemStateActivatedEvent(Guid StateId) : DomainEvent;
public sealed record SystemSnapshotCapturedEvent(Guid StateId, long EventStoreVersion, int ActiveAggregates, string SnapshotHash) : DomainEvent;
public sealed record StateValidationRecordedEvent(Guid StateId, string SystemName, bool IsValid, string Details) : DomainEvent;
public sealed record SystemStateDeclaredAuthoritativeEvent(Guid StateId, string SnapshotHash) : DomainEvent;
public sealed record SystemStateDegradedEvent(Guid StateId, string Reason) : DomainEvent;
