namespace Whycespace.Domain.BusinessSystem.Integration.Callback;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(CallbackStatus status)
    {
        return status == CallbackStatus.Pending;
    }
}
