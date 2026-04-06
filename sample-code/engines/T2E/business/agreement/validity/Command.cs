namespace Whycespace.Engines.T2E.Business.Agreement.Validity;

public record ValidityCommand(
    string Action,
    string EntityId,
    object Payload
);
