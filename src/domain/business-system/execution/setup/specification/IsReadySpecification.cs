namespace Whycespace.Domain.BusinessSystem.Execution.Setup;

public sealed class IsReadySpecification
{
    public bool IsSatisfiedBy(SetupStatus status)
    {
        return status == SetupStatus.Ready;
    }
}
