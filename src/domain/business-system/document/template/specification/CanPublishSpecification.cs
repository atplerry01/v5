namespace Whycespace.Domain.BusinessSystem.Document.Template;

public sealed class CanPublishSpecification
{
    public bool IsSatisfiedBy(TemplateStatus status)
    {
        return status == TemplateStatus.Draft;
    }
}
