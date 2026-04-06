namespace Whycespace.Engines.T2E.Business.Integration.Callback;

public class CallbackEngine
{
    private readonly CallbackPolicyAdapter _policy;

    public CallbackEngine(CallbackPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CallbackResult> ExecuteAsync(CallbackCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CallbackResult(true, "Executed");
    }
}
