namespace Whycespace.Domain.BusinessSystem.Execution.Completion;

public sealed class CanFailSpecification
{
    public bool IsSatisfiedBy(CompletionStatus status)
    {
        return status == CompletionStatus.Pending;
    }
}
