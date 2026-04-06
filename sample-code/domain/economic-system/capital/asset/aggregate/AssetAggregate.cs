using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Asset;

public class AssetAggregate : AggregateRoot
{
    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new AssetCreatedEvent(id));
    }
}
