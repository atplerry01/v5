namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(CounterpartyStatus status)
    {
        return status == CounterpartyStatus.Active;
    }
}
