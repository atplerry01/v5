namespace Whycespace.Domain.BusinessSystem.Execution.Setup;

public sealed class CanMarkReadySpecification
{
    public bool IsSatisfiedBy(SetupStatus status)
    {
        return status == SetupStatus.Configured;
    }
}
