using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public class SPVAggregate : AggregateRoot
{
    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new SPVCreatedEvent(id));
    }
}
