namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed class AccessProfileActiveSpec : Specification<AccessProfileAggregate>
{
    public override bool IsSatisfiedBy(AccessProfileAggregate entity)
        => entity.Status == AccessProfileStatus.Active
           && entity.Roles.Any(r => r.IsActive);
}
