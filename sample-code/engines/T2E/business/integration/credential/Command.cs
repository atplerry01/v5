namespace Whycespace.Engines.T2E.Business.Integration.Credential;

public record CredentialCommand(
    string Action,
    string EntityId,
    object Payload
);
