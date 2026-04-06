namespace Whycespace.Engines.T2E.Business.Billing.BillRun;

public class BillRunEngine
{
    private readonly BillRunPolicyAdapter _policy;

    public BillRunEngine(BillRunPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<BillRunResult> ExecuteAsync(BillRunCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new BillRunResult(true, "Executed");
    }
}
