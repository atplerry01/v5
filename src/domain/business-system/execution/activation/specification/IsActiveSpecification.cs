namespace Whycespace.Domain.BusinessSystem.Execution.Activation;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(ActivationStatus status)
    {
        return status == ActivationStatus.Active;
    }
}
