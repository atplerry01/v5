using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Document.Record;

public class RecordAggregate : AggregateRoot
{
    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new RecordCreatedEvent(id));
    }
}
