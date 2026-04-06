namespace Whycespace.Engines.T2E.Business.Execution.Charge;

public class ChargeEngine
{
    private readonly ChargePolicyAdapter _policy;

    public ChargeEngine(ChargePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ChargeResult> ExecuteAsync(ChargeCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ChargeResult(true, "Executed");
    }
}
