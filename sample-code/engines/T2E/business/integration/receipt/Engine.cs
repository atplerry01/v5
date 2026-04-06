namespace Whycespace.Engines.T2E.Business.Integration.Receipt;

public class ReceiptEngine
{
    private readonly ReceiptPolicyAdapter _policy;

    public ReceiptEngine(ReceiptPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ReceiptResult> ExecuteAsync(ReceiptCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ReceiptResult(true, "Executed");
    }
}
