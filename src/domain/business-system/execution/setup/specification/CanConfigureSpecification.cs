namespace Whycespace.Domain.BusinessSystem.Execution.Setup;

public sealed class CanConfigureSpecification
{
    public bool IsSatisfiedBy(SetupStatus status)
    {
        return status == SetupStatus.Pending;
    }
}
