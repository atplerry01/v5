namespace Whycespace.Engines.T2E.Business.Portfolio.Holding;

public class HoldingEngine
{
    private readonly HoldingPolicyAdapter _policy;

    public HoldingEngine(HoldingPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<HoldingResult> ExecuteAsync(HoldingCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new HoldingResult(true, "Executed");
    }
}
