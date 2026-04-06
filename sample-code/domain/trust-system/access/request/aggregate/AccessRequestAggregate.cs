namespace Whycespace.Domain.TrustSystem.Access.Request;

public sealed class AccessRequestAggregate : AggregateRoot
{
    public Guid RequesterId { get; private set; }
    public string Resource { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public AccessRequestStatus Status { get; private set; } = AccessRequestStatus.Pending;
    public Guid? ApproverId { get; private set; }
    public string Justification { get; private set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; private set; }

    private AccessRequestAggregate() { }

    public static AccessRequestAggregate Submit(
        Guid id,
        Guid requesterId,
        string resource,
        string action,
        string justification,
        DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(id);
        Guard.AgainstDefault(requesterId);
        Guard.AgainstEmpty(resource);
        Guard.AgainstEmpty(action);

        var aggregate = new AccessRequestAggregate
        {
            Id = id,
            RequesterId = requesterId,
            Resource = resource,
            Action = action,
            Justification = justification,
            Status = AccessRequestStatus.Pending,
            RequestedAt = timestamp
        };

        aggregate.RaiseDomainEvent(new AccessRequestSubmittedEvent(id, requesterId, resource, action, justification));
        return aggregate;
    }

    public void Approve(Guid approverId, DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(approverId);
        EnsureInvariant(Status == AccessRequestStatus.Pending, "REQUEST_MUST_BE_PENDING", "Only pending requests can be approved.");

        Status = AccessRequestStatus.Approved;
        ApproverId = approverId;
        RaiseDomainEvent(new AccessRequestApprovedEvent(Id, RequesterId, approverId, Resource, Action));
    }

    public void Reject(Guid approverId, string reason, DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(approverId);
        EnsureInvariant(Status == AccessRequestStatus.Pending, "REQUEST_MUST_BE_PENDING", "Only pending requests can be rejected.");

        Status = AccessRequestStatus.Rejected;
        ApproverId = approverId;
        RaiseDomainEvent(new AccessRequestRejectedEvent(Id, RequesterId, approverId, reason));
    }
}
