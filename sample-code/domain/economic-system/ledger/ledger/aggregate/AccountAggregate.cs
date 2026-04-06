using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public class AccountAggregate : AggregateRoot
{
    public JurisdictionId? JurisdictionId { get; private set; }

    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new AccountCreatedEvent(id));
    }
}
