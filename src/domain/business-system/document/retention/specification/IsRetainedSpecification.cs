namespace Whycespace.Domain.BusinessSystem.Document.Retention;

public sealed class IsRetainedSpecification
{
    public bool IsSatisfiedBy(RetentionStatus status)
    {
        return status == RetentionStatus.Retained;
    }
}
