namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed class ServiceIdentityActiveSpec : Specification<ServiceIdentityAggregate>
{
    public override bool IsSatisfiedBy(ServiceIdentityAggregate entity)
        => entity.Status == ServiceIdentityStatus.Active;
}
