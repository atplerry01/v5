namespace Whycespace.Engines.T2E.Business.Inventory.Batch;

public class BatchEngine
{
    private readonly BatchPolicyAdapter _policy;

    public BatchEngine(BatchPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<BatchResult> ExecuteAsync(BatchCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new BatchResult(true, "Executed");
    }
}
