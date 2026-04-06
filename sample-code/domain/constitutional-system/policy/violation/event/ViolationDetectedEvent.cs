namespace Whycespace.Domain.ConstitutionalSystem.Policy.Violation;

using Whycespace.Domain.SharedKernel;

public sealed record ViolationDetectedEvent(
    Guid ViolationId,
    Guid PolicyRuleId,
    Guid ConstraintId,
    string Severity,
    string TargetEntity) : DomainEvent;
