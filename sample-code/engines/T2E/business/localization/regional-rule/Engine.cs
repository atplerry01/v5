namespace Whycespace.Engines.T2E.Business.Localization.RegionalRule;

public class RegionalRuleEngine
{
    private readonly RegionalRulePolicyAdapter _policy;

    public RegionalRuleEngine(RegionalRulePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RegionalRuleResult> ExecuteAsync(RegionalRuleCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RegionalRuleResult(true, "Executed");
    }
}
