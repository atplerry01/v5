using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationException;

public sealed record ReconciliationExceptionCreatedEvent(Guid ExceptionId, string Description, string Severity) : DomainEvent;
public sealed record ReconciliationExceptionResolvedEvent(Guid ExceptionId, string Resolution) : DomainEvent;
