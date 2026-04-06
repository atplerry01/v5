using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

public sealed record VerificationSessionStartedEvent(Guid SessionId, string ScopeType, string TargetSystem) : DomainEvent;
public sealed record ConsistencyCheckRecordedEvent(Guid SessionId, string CheckName, bool IsConsistent, string Details) : DomainEvent;
public sealed record VerificationSessionCompletedEvent(Guid SessionId, bool AllConsistent, int TotalChecks, int FailedChecks) : DomainEvent;
public sealed record VerificationSessionFailedEvent(Guid SessionId, string Reason) : DomainEvent;
