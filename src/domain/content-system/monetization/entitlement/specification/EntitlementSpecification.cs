using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Entitlement;

public sealed class EntitlementSpecification : Specification<EntitlementStatus>
{
    public override bool IsSatisfiedBy(EntitlementStatus entity) =>
        entity == EntitlementStatus.Granted || entity == EntitlementStatus.Extended;

    public void EnsureActive(EntitlementStatus status)
    {
        if (status == EntitlementStatus.Revoked) throw EntitlementErrors.AlreadyRevoked();
    }

    public void EnsureValidity(Timestamp from, Timestamp until)
    {
        if (until.Value <= from.Value) throw EntitlementErrors.InvalidValidity();
    }
}
