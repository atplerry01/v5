namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed class AuthorizationService
{
    public AuthorizationAggregate EvaluateAccess(
        Guid id,
        Guid identityId,
        string resource,
        string action,
        string policyId,
        bool isAllowed,
        DateTimeOffset timestamp)
    {
        var aggregate = AuthorizationAggregate.Evaluate(id, identityId, resource, action, policyId, timestamp);

        if (isAllowed)
            aggregate.Approve(timestamp);
        else
            aggregate.Deny("Policy evaluation denied access", timestamp);

        return aggregate;
    }
}
