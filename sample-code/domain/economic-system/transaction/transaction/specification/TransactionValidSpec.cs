namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed class TransactionValidSpec : Specification<TransactionAggregate>
{
    public override bool IsSatisfiedBy(TransactionAggregate entity) =>
        !entity.DomainEvents.OfType<TransactionRejectedEvent>().Any();
}
