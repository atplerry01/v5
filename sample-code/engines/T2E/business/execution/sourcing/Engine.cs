namespace Whycespace.Engines.T2E.Business.Execution.Sourcing;

public class SourcingEngine
{
    private readonly SourcingPolicyAdapter _policy;

    public SourcingEngine(SourcingPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SourcingResult> ExecuteAsync(SourcingCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SourcingResult(true, "Executed");
    }
}
