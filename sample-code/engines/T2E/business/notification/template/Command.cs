namespace Whycespace.Engines.T2E.Business.Notification.Template;

public record TemplateCommand(
    string Action,
    string EntityId,
    object Payload
);
