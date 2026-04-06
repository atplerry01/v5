namespace Whycespace.Engines.T2E.Business.Integration.Export;

public record ExportCommand(
    string Action,
    string EntityId,
    object Payload
);
