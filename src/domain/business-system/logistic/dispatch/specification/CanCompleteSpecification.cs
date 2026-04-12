namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(DispatchStatus status)
    {
        return status == DispatchStatus.Released;
    }
}
