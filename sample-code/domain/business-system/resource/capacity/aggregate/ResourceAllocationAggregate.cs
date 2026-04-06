using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public class ResourceAllocationAggregate : AggregateRoot
{
    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new AllocationCreatedEvent(id));
    }
}
