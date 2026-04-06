namespace Whycespace.Engines.T2E.Decision.Governance.Guardian;

public class GuardianEngine
{
    private readonly GuardianPolicyAdapter _policy;

    public GuardianEngine(GuardianPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<GuardianResult> ExecuteAsync(GuardianCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new GuardianResult(true, "Executed");
    }
}
