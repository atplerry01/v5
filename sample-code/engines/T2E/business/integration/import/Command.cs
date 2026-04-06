namespace Whycespace.Engines.T2E.Business.Integration.Import;

public record ImportCommand(
    string Action,
    string EntityId,
    object Payload
);
