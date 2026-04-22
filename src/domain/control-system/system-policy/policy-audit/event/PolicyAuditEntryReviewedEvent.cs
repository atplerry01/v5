using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyAudit;

public sealed record PolicyAuditEntryReviewedEvent(
    PolicyAuditId Id,
    string ReviewerId,
    string Reason) : DomainEvent;
