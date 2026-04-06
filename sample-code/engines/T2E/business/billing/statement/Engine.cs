namespace Whycespace.Engines.T2E.Business.Billing.Statement;

public class StatementEngine
{
    private readonly StatementPolicyAdapter _policy;

    public StatementEngine(StatementPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<StatementResult> ExecuteAsync(StatementCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new StatementResult(true, "Executed");
    }
}
