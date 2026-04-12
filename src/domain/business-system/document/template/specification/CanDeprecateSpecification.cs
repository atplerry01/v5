namespace Whycespace.Domain.BusinessSystem.Document.Template;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(TemplateStatus status)
    {
        return status == TemplateStatus.Published;
    }
}
