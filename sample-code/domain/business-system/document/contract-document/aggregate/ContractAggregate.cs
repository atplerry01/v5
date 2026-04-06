using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public class ContractAggregate : AggregateRoot
{
    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new ContractCreatedEvent(id));
    }
}
