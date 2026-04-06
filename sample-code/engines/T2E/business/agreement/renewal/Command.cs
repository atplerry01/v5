namespace Whycespace.Engines.T2E.Business.Agreement.Renewal;

public record RenewalCommand(
    string Action,
    string EntityId,
    object Payload
);
