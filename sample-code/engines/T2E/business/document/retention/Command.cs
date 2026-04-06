namespace Whycespace.Engines.T2E.Business.Document.Retention;

public record RetentionCommand(
    string Action,
    string EntityId,
    object Payload
);
