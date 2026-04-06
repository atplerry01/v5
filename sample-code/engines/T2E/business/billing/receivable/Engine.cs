namespace Whycespace.Engines.T2E.Business.Billing.Receivable;

public class ReceivableEngine
{
    private readonly ReceivablePolicyAdapter _policy;

    public ReceivableEngine(ReceivablePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ReceivableResult> ExecuteAsync(ReceivableCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ReceivableResult(true, "Executed");
    }
}
