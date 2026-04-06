using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public class EvidenceAggregate : AggregateRoot
{
    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new EvidenceCreatedEvent(id));
    }
}
