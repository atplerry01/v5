namespace Whycespace.Domain.BusinessSystem.Execution.Activation;

public sealed class CanDeactivateSpecification
{
    public bool IsSatisfiedBy(ActivationStatus status)
    {
        return status == ActivationStatus.Active;
    }
}
