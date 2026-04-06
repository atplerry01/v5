namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed class IdentityValidSpec : Specification<IdentityAggregate>
{
    public override bool IsSatisfiedBy(IdentityAggregate entity)
        => entity.IdentityId is not null
           && entity.IdentityType is not null
           && !string.IsNullOrWhiteSpace(entity.DisplayName)
           && entity.Status != IdentityStatus.Deactivated;
}
