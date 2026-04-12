namespace Whycespace.Domain.BusinessSystem.Integration.Adapter;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(AdapterStatus status)
    {
        return status == AdapterStatus.Active;
    }
}
