namespace Whycespace.Engines.T2E.Business.Agreement.Amendment;

public record AmendmentCommand(
    string Action,
    string EntityId,
    object Payload
);
