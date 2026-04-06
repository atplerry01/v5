namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed class BindingActiveSpec : Specification<IdentityEconomicBindingAggregate>
{
    public override bool IsSatisfiedBy(IdentityEconomicBindingAggregate entity) =>
        !entity.DomainEvents.OfType<IdentityWalletUnboundEvent>().Any();
}
