namespace Whycespace.Domain.BusinessSystem.Execution.Activation;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ActivationStatus status)
    {
        return status == ActivationStatus.Pending;
    }
}
