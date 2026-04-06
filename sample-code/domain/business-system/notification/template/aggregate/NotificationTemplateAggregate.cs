using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Template;

public sealed class NotificationTemplateAggregate : AggregateRoot
{
    public TemplateId TemplateId { get; private set; }
    public TemplateType TemplateType { get; private set; } = default!;
    public string Content { get; private set; } = string.Empty;
    public string Locale { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public static NotificationTemplateAggregate Create(Guid templateId, TemplateType templateType, string content, string locale)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException(TemplateErrors.InvalidContent, "Template content cannot be empty.");

        if (string.IsNullOrWhiteSpace(locale))
            throw new DomainException(TemplateErrors.InvalidLocale, "Template locale cannot be empty.");

        var template = new NotificationTemplateAggregate();
        template.Apply(new TemplateCreatedEvent(templateId, templateType.Value, content, locale));
        return template;
    }

    private void Apply(TemplateCreatedEvent e)
    {
        Id = e.TemplateId;
        TemplateId = new TemplateId(e.TemplateId);
        TemplateType = new TemplateType(e.TemplateType);
        Content = e.Content;
        Locale = e.Locale;
        CreatedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }
}
