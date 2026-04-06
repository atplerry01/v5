namespace Whycespace.Engines.T2E.Business.Integration.Token;

public record TokenCommand(
    string Action,
    string EntityId,
    object Payload
);
