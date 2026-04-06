namespace Whycespace.Engines.T2E.Business.Inventory.Transfer;

public class TransferEngine
{
    private readonly TransferPolicyAdapter _policy;

    public TransferEngine(TransferPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<TransferResult> ExecuteAsync(TransferCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new TransferResult(true, "Executed");
    }
}
