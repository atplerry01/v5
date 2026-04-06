namespace Whycespace.Engines.T2E.Business.Document.Version;

public record VersionCommand(
    string Action,
    string EntityId,
    object Payload
);
