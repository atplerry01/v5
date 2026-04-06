namespace Whycespace.Engines.T2E.Business.Integration.Gateway;

public record GatewayCommand(
    string Action,
    string EntityId,
    object Payload
);
