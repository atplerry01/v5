using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

public static class DocumentTemplateErrors
{
    public static DomainException TemplateArchived()
        => new("Cannot mutate an archived template.");

    public static DomainException AlreadyActive()
        => new("Template is already active.");

    public static DomainException AlreadyDeprecated()
        => new("Template is already deprecated.");

    public static DomainException AlreadyArchived()
        => new("Template is already archived.");

    public static DomainException CannotActivateDeprecated()
        => new("Cannot activate a deprecated template.");

    public static DomainException InvalidDeprecationReason()
        => new("Deprecation reason cannot be empty.");
}
