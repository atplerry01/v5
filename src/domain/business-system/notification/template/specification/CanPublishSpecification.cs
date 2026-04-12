namespace Whycespace.Domain.BusinessSystem.Notification.Template;

public sealed class CanPublishSpecification
{
    public bool IsSatisfiedBy(TemplateStatus status)
    {
        return status == TemplateStatus.Draft;
    }
}
