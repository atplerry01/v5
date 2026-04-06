using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Emergency;

public sealed record EmergencyControlCreatedEvent(Guid ControlId, string ScopeType, string TargetId) : DomainEvent;
public sealed record EmergencyHaltActivatedEvent(Guid ControlId, string ScopeType, string TargetId, string Reason, string InitiatedBy) : DomainEvent;
public sealed record EmergencyHaltResolvedEvent(Guid ControlId, string ResolvedBy, string Resolution) : DomainEvent;
