namespace Whycespace.Engines.T2E.Business.Integration.Endpoint;

public record EndpointCommand(
    string Action,
    string EntityId,
    object Payload
);
