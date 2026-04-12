namespace Whycespace.Domain.BusinessSystem.Execution.Completion;

public sealed class IsCompletedSpecification
{
    public bool IsSatisfiedBy(CompletionStatus status)
    {
        return status == CompletionStatus.Completed;
    }
}
