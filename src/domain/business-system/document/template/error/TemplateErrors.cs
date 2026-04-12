namespace Whycespace.Domain.BusinessSystem.Document.Template;

public static class TemplateErrors
{
    public static TemplateDomainException MissingId()
        => new("TemplateId is required and must not be empty.");

    public static TemplateDomainException InvalidStateTransition(TemplateStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static TemplateDomainException StructureRequired()
        => new("Template must contain at least one structure definition.");

    public static TemplateDomainException ModificationAfterPublish()
        => new("Cannot modify a published template.");
}
