namespace Whycespace.Domain.BusinessSystem.Agreement.Counterparty;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(CounterpartyStatus status)
    {
        return status == CounterpartyStatus.Active;
    }
}
