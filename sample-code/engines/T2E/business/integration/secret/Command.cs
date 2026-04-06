namespace Whycespace.Engines.T2E.Business.Integration.Secret;

public record SecretCommand(
    string Action,
    string EntityId,
    object Payload
);
