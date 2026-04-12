namespace Whycespace.Domain.BusinessSystem.Document.Retention;

public sealed class CanExpireSpecification
{
    public bool IsSatisfiedBy(RetentionStatus status)
    {
        return status == RetentionStatus.Retained;
    }
}
