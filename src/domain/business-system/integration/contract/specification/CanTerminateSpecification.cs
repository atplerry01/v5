namespace Whycespace.Domain.BusinessSystem.Integration.Contract;

public sealed class CanTerminateSpecification
{
    public bool IsSatisfiedBy(ContractStatus status)
    {
        return status == ContractStatus.Active;
    }
}
