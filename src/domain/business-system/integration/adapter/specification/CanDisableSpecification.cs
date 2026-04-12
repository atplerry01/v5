namespace Whycespace.Domain.BusinessSystem.Integration.Adapter;

public sealed class CanDisableSpecification
{
    public bool IsSatisfiedBy(AdapterStatus status)
    {
        return status == AdapterStatus.Active;
    }
}
