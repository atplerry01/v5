namespace Whycespace.Engines.T2E.Business.Portfolio.Allocation;

public class AllocationEngine
{
    private readonly AllocationPolicyAdapter _policy;

    public AllocationEngine(AllocationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AllocationResult> ExecuteAsync(AllocationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AllocationResult(true, "Executed");
    }
}
