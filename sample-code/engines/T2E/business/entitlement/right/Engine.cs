namespace Whycespace.Engines.T2E.Business.Entitlement.Right;

public class RightEngine
{
    private readonly RightPolicyAdapter _policy;

    public RightEngine(RightPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RightResult> ExecuteAsync(RightCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RightResult(true, "Executed");
    }
}
