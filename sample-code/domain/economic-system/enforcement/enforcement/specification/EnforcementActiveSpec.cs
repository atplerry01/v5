namespace Whycespace.Domain.EconomicSystem.Enforcement.Enforcement;

public sealed class EnforcementActiveSpec : Specification<EconomicEnforcementAggregate>
{
    public override bool IsSatisfiedBy(EconomicEnforcementAggregate entity) =>
        !entity.DomainEvents.OfType<EnforcementReleasedEvent>().Any();
}
