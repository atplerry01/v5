namespace Whycespace.Engines.T2E.Business.Integration.Client;

public record ClientCommand(
    string Action,
    string EntityId,
    object Payload
);
