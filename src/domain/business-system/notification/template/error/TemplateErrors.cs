namespace Whycespace.Domain.BusinessSystem.Notification.Template;

public static class TemplateErrors
{
    public static TemplateDomainException MissingId()
        => new("TemplateId is required and must not be empty.");

    public static TemplateDomainException InvalidContent()
        => new("Template must define valid subject and body content.");

    public static TemplateDomainException InvalidStateTransition(TemplateStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static TemplateDomainException ImmutableAfterPublish()
        => new("Template content cannot be modified after publication.");
}

public sealed class TemplateDomainException : Exception
{
    public TemplateDomainException(string message) : base(message) { }
}
