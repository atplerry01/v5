namespace Whycespace.Domain.BusinessSystem.Document.Template;

public sealed class IsPublishedSpecification
{
    public bool IsSatisfiedBy(TemplateStatus status)
    {
        return status == TemplateStatus.Published;
    }
}
