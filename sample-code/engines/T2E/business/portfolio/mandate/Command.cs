namespace Whycespace.Engines.T2E.Business.Portfolio.Mandate;

public record MandateCommand(
    string Action,
    string EntityId,
    object Payload
);
