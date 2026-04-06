namespace Whycespace.Engines.T2E.Business.Subscription.Plan;

public class PlanEngine
{
    private readonly PlanPolicyAdapter _policy;

    public PlanEngine(PlanPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<PlanResult> ExecuteAsync(PlanCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new PlanResult(true, "Executed");
    }
}
