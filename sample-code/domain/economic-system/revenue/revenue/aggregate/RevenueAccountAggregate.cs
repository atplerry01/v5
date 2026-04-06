using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public class RevenueAccountAggregate : AggregateRoot
{
    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new RevenueAccountCreatedEvent(id));
    }
}
