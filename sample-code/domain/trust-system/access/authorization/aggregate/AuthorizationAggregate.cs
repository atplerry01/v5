namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed class AuthorizationAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public string Resource { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public AuthorizationDecision Decision { get; private set; } = AuthorizationDecision.Pending;
    public string PolicyId { get; private set; } = string.Empty;
    public DateTimeOffset EvaluatedAt { get; private set; }

    private AuthorizationAggregate() { }

    public static AuthorizationAggregate Evaluate(
        Guid id,
        Guid identityId,
        string resource,
        string action,
        string policyId,
        DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(id);
        Guard.AgainstDefault(identityId);
        Guard.AgainstEmpty(resource);
        Guard.AgainstEmpty(action);

        var aggregate = new AuthorizationAggregate
        {
            Id = id,
            IdentityId = identityId,
            Resource = resource,
            Action = action,
            PolicyId = policyId,
            Decision = AuthorizationDecision.Pending,
            EvaluatedAt = timestamp
        };

        aggregate.RaiseDomainEvent(new AuthorizationEvaluatedEvent(id, identityId, resource, action, policyId));
        return aggregate;
    }

    public void Approve(DateTimeOffset timestamp)
    {
        EnsureInvariant(
            Decision == AuthorizationDecision.Pending,
            "AUTH_MUST_BE_PENDING",
            "Only pending authorizations can be approved.");

        Decision = AuthorizationDecision.Approved;
        EvaluatedAt = timestamp;
        RaiseDomainEvent(new AuthorizationApprovedEvent(Id, IdentityId, Resource, Action));
    }

    public void Deny(string reason, DateTimeOffset timestamp)
    {
        EnsureInvariant(
            Decision == AuthorizationDecision.Pending,
            "AUTH_MUST_BE_PENDING",
            "Only pending authorizations can be denied.");

        Decision = AuthorizationDecision.Denied;
        EvaluatedAt = timestamp;
        RaiseDomainEvent(new AuthorizationDeniedEvent(Id, IdentityId, Resource, Action, reason));
    }
}
