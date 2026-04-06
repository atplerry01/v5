namespace Whycespace.Engines.T2E.Business.Agreement.Signature;

public record SignatureCommand(
    string Action,
    string EntityId,
    object Payload
);
