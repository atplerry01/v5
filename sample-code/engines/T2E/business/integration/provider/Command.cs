namespace Whycespace.Engines.T2E.Business.Integration.Provider;

public record ProviderCommand(
    string Action,
    string EntityId,
    object Payload
);
