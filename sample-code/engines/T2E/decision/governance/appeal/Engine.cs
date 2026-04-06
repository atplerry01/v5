namespace Whycespace.Engines.T2E.Decision.Governance.Appeal;

public class AppealEngine
{
    private readonly AppealPolicyAdapter _policy;

    public AppealEngine(AppealPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AppealResult> ExecuteAsync(AppealCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AppealResult(true, "Executed");
    }
}
