namespace Whycespace.Engines.T2E.Business.Logistic.Dispatch;

public class DispatchEngine
{
    private readonly DispatchPolicyAdapter _policy;

    public DispatchEngine(DispatchPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<DispatchResult> ExecuteAsync(DispatchCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new DispatchResult(true, "Executed");
    }
}
