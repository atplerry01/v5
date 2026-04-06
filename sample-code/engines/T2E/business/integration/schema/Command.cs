namespace Whycespace.Engines.T2E.Business.Integration.Schema;

public record SchemaCommand(
    string Action,
    string EntityId,
    object Payload
);
