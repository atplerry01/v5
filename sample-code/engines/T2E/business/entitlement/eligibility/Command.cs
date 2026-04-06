namespace Whycespace.Engines.T2E.Business.Entitlement.Eligibility;

public record EligibilityCommand(
    string Action,
    string EntityId,
    object Payload
);
