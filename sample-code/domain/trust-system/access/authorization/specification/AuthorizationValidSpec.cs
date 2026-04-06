namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed class AuthorizationValidSpec : Specification<AuthorizationAggregate>
{
    public override bool IsSatisfiedBy(AuthorizationAggregate entity)
    {
        return entity.Decision == AuthorizationDecision.Approved
            && !string.IsNullOrWhiteSpace(entity.Resource)
            && !string.IsNullOrWhiteSpace(entity.Action);
    }
}
