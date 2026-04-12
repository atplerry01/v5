namespace Whycespace.Domain.BusinessSystem.Document.Retention;

public sealed class CanRetainSpecification
{
    public bool IsSatisfiedBy(RetentionStatus status)
    {
        return status == RetentionStatus.Active;
    }
}
