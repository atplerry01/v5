namespace Whycespace.Engines.T2E.Business.Document.Template;

public record TemplateCommand(
    string Action,
    string EntityId,
    object Payload
);
