namespace Whycespace.Domain.BusinessSystem.Integration.Callback;

public sealed class IsCompletedSpecification
{
    public bool IsSatisfiedBy(CallbackStatus status)
    {
        return status == CallbackStatus.Completed;
    }
}
