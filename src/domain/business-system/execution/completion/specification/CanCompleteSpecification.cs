namespace Whycespace.Domain.BusinessSystem.Execution.Completion;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(CompletionStatus status)
    {
        return status == CompletionStatus.Pending;
    }
}
