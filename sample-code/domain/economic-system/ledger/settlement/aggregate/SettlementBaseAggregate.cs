using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public class SettlementBaseAggregate : AggregateRoot
{
    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new SettlementCreatedEvent(id));
    }
}
