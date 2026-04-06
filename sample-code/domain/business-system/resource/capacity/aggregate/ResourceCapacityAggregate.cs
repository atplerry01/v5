using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Resource.Capacity;

public class ResourceCapacityAggregate : AggregateRoot
{
    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new CapacityCreatedEvent(id));
    }
}
