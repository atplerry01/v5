namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed class IdentityGraphActiveSpec : Specification<IdentityGraphAggregate>
{
    public override bool IsSatisfiedBy(IdentityGraphAggregate entity)
        => entity.Status == IdentityGraphStatus.Active;
}
