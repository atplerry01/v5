namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed class WalletActiveSpec : Specification<WalletAggregate>
{
    public override bool IsSatisfiedBy(WalletAggregate entity) => entity.DomainEvents
        .OfType<WalletFrozenEvent>().Count() <= entity.DomainEvents.OfType<WalletUnfrozenEvent>().Count();
}
