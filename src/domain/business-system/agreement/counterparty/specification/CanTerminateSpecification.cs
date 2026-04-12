namespace Whycespace.Domain.BusinessSystem.Agreement.Counterparty;

public sealed class CanTerminateSpecification
{
    public bool IsSatisfiedBy(CounterpartyStatus status)
    {
        return status == CounterpartyStatus.Active || status == CounterpartyStatus.Suspended;
    }
}
