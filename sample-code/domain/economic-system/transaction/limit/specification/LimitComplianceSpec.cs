namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed class LimitComplianceSpec : Specification<EconomicLimitAggregate>
{
    public override bool IsSatisfiedBy(EconomicLimitAggregate entity) =>
        !entity.DomainEvents.OfType<LimitExceededEvent>().Any();
}
