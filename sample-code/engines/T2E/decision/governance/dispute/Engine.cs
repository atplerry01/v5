namespace Whycespace.Engines.T2E.Decision.Governance.Dispute;

public class DisputeEngine
{
    private readonly DisputePolicyAdapter _policy;

    public DisputeEngine(DisputePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<DisputeResult> ExecuteAsync(DisputeCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new DisputeResult(true, "Executed");
    }
}
