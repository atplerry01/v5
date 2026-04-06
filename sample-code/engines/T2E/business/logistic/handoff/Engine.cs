namespace Whycespace.Engines.T2E.Business.Logistic.Handoff;

public class HandoffEngine
{
    private readonly HandoffPolicyAdapter _policy;

    public HandoffEngine(HandoffPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<HandoffResult> ExecuteAsync(HandoffCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new HandoffResult(true, "Executed");
    }
}
