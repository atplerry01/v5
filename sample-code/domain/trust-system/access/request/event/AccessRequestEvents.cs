namespace Whycespace.Domain.TrustSystem.Access.Request;

public sealed record AccessRequestSubmittedEvent(
    Guid RequestId,
    Guid RequesterId,
    string Resource,
    string Action,
    string Justification) : DomainEvent;

public sealed record AccessRequestApprovedEvent(
    Guid RequestId,
    Guid RequesterId,
    Guid ApproverId,
    string Resource,
    string Action) : DomainEvent;

public sealed record AccessRequestRejectedEvent(
    Guid RequestId,
    Guid RequesterId,
    Guid ApproverId,
    string Reason) : DomainEvent;
