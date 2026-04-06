namespace Whycespace.Domain.TrustSystem.Access.Session;

public sealed class SessionActiveSpec : Specification<SessionAggregate>
{
    public override bool IsSatisfiedBy(SessionAggregate entity)
    {
        return entity.Status == SessionStatus.Active;
    }
}
