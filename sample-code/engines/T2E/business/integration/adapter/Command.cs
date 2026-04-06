namespace Whycespace.Engines.T2E.Business.Integration.Adapter;

public record AdapterCommand(
    string Action,
    string EntityId,
    object Payload
);
