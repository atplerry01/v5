namespace Whycespace.Domain.BusinessSystem.Agreement.Contract;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(ContractStatus status)
    {
        return status == ContractStatus.Active;
    }
}
