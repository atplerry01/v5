using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationItem;

public sealed record ReconciliationItemCreatedEvent(Guid ItemId, string SourceRef, string TargetRef, decimal Amount) : DomainEvent;
public sealed record ReconciliationItemMatchedEvent(Guid ItemId) : DomainEvent;
