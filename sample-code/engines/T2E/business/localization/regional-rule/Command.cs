namespace Whycespace.Engines.T2E.Business.Localization.RegionalRule;

public record RegionalRuleCommand(
    string Action,
    string EntityId,
    object Payload
);
