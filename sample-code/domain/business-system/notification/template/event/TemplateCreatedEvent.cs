using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Template;

public sealed record TemplateCreatedEvent(
    Guid TemplateId,
    string TemplateType,
    string Content,
    string Locale
) : DomainEvent;
