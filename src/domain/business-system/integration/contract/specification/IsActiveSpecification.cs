namespace Whycespace.Domain.BusinessSystem.Integration.Contract;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(ContractStatus status)
    {
        return status == ContractStatus.Active;
    }
}
