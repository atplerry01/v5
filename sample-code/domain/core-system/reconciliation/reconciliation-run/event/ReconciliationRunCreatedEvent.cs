using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationRun;

public sealed record ReconciliationRunCreatedEvent(Guid RunId, string RunType, string Scope) : DomainEvent;
public sealed record ReconciliationRunCompletedEvent(Guid RunId) : DomainEvent;
