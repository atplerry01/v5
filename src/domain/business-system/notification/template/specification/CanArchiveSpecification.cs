namespace Whycespace.Domain.BusinessSystem.Notification.Template;

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(TemplateStatus status)
    {
        return status == TemplateStatus.Published;
    }
}
